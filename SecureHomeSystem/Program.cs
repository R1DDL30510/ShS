using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using SecureHomeSystem.Configuration;
using SecureHomeSystem.Infrastructure;
using SecureHomeSystem.Services;
using Serilog;

namespace SecureHomeSystem;

/// <summary>
/// Entry point for the SecureHomeSystem worker host. The bootstrapper configures
/// strongly typed options, registers the background worker, and exposes lightweight
/// HTTP endpoints for health monitoring.
/// </summary>
public static class Program
{
    /// <summary>
    /// Builds the web application host, runs it, and ensures Serilog flushes buffers
    /// during shutdown.
    /// </summary>
    /// <param name="args">Command-line arguments forwarded by the hosting infrastructure.</param>
    public static void Main(string[] args)
    {
        var app = BuildWebApplication(args);

        try
        {
            app.Run();
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    /// <summary>
    /// Configures and builds the ASP.NET Core web application for hosting the worker.
    /// Tests call this method to verify service registration without starting Kestrel.
    /// </summary>
    /// <param name="args">Command-line arguments forwarded by the hosting infrastructure.</param>
    internal static WebApplication BuildWebApplication(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Host.UseSerilog((context, services, loggerConfiguration) =>
        {
            loggerConfiguration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext();
        });

        var configuration = builder.Configuration;

        RegisterOptions(builder.Services, configuration);

        var healthOptions = BindAndValidate<HealthOptions>(configuration, "Health");
        var logCollectorOptions = BindAndValidate<LogCollectorOptions>(configuration, "LogCollector");
        var logStorageOptions = BindAndValidate<LogStorageOptions>(configuration, "LogStorage");

        EnsureLogDirectories(logStorageOptions);

        var healthPath = NormalizePath(healthOptions.HealthPath, "/health");
        var livenessPath = NormalizePath(healthOptions.LivenessPath, "/live");

        builder.Services
            .AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy());

        builder.Services.AddSingleton<IProcessRunner, ProcessRunner>();
        builder.Services.AddSingleton<IDockerServiceDetector, DockerServiceDetector>();
        builder.Services.AddHostedService<Worker>();

        if (logCollectorOptions.Enabled)
        {
            builder.Services.AddHostedService<ContainerLogCollector>();
        }

        builder.WebHost.ConfigureKestrel(options =>
        {
            options.ListenAnyIP(healthOptions.Port);
        });

        var app = builder.Build();

        app.MapHealthChecks(healthPath);
        app.MapGet(livenessPath, () => Results.Ok(new { status = "Healthy" }));

        return app;
    }

    private static void RegisterOptions(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IValidateOptions<DockerOptions>, DockerOptionsValidator>();
        services.AddSingleton<IValidateOptions<ServiceEndpointsOptions>, ServiceEndpointsOptionsValidator>();
        services.AddSingleton<IValidateOptions<LogCollectorOptions>, LogCollectorOptionsValidator>();

        services
            .AddOptions<DockerOptions>()
            .Bind(configuration.GetSection("Docker"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services
            .AddOptions<ServiceEndpointsOptions>()
            .Bind(configuration.GetSection("Services"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services
            .AddOptions<ResourceSchedulerOptions>()
            .Bind(configuration.GetSection("ResourceScheduler"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services
            .AddOptions<HealthOptions>()
            .Bind(configuration.GetSection("Health"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services
            .AddOptions<LogStorageOptions>()
            .Bind(configuration.GetSection("LogStorage"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services
            .AddOptions<LogCollectorOptions>()
            .Bind(configuration.GetSection("LogCollector"))
            .ValidateDataAnnotations()
            .ValidateOnStart();
    }

    private static T BindAndValidate<T>(IConfiguration configuration, string sectionName)
        where T : class, new()
    {
        var instance = configuration.GetSection(sectionName).Get<T>() ?? new T();
        ValidateObjectGraph(instance);
        return instance;
    }

    private static void ValidateObjectGraph(object instance)
    {
        Validator.ValidateObject(instance, new ValidationContext(instance), validateAllProperties: true);

        foreach (var property in instance.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            var value = property.GetValue(instance);
            if (value is null)
            {
                continue;
            }

            if (property.PropertyType == typeof(string) || property.PropertyType.IsValueType)
            {
                continue;
            }

            if (value is IEnumerable enumerable)
            {
                foreach (var item in enumerable)
                {
                    if (item is null || item is string || item.GetType().IsValueType)
                    {
                        continue;
                    }

                    ValidateObjectGraph(item);
                }

                continue;
            }

            ValidateObjectGraph(value);
        }
    }

    /// <summary>
    /// Normalises user-provided paths by enforcing a single leading slash.
    /// </summary>
    /// <param name="candidate">Candidate path value supplied through configuration.</param>
    /// <param name="fallback">Fallback path used when the candidate is null or whitespace.</param>
    internal static string NormalizePath(string? candidate, string fallback)
    {
        if (string.IsNullOrWhiteSpace(candidate))
        {
            return fallback;
        }

        return candidate.StartsWith('/') ? candidate : $"/{candidate}";
    }

    /// <summary>
    /// Ensures the worker's log directories exist prior to starting the host.
    /// </summary>
    /// <param name="options">Resolved storage options.</param>
    internal static void EnsureLogDirectories(LogStorageOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.RootPath))
        {
            return;
        }

        var directories = new[]
        {
            options.RootPath,
            Path.Combine(options.RootPath, options.WorkerFolderName),
            Path.Combine(options.RootPath, options.ServicesFolderName),
            Path.Combine(options.RootPath, options.StateFolderName)
        };

        foreach (var directory in directories.Where(path => !string.IsNullOrWhiteSpace(path)))
        {
            Directory.CreateDirectory(directory);
        }
    }
}
