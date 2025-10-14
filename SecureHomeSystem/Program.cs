using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
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

        builder.Services.Configure<DockerOptions>(builder.Configuration.GetSection("Docker"));
        builder.Services.Configure<ServiceEndpointsOptions>(builder.Configuration.GetSection("Services"));
        builder.Services.Configure<ResourceSchedulerOptions>(builder.Configuration.GetSection("ResourceScheduler"));
        builder.Services.Configure<HealthOptions>(builder.Configuration.GetSection("Health"));
        builder.Services.Configure<LogStorageOptions>(builder.Configuration.GetSection("LogStorage"));
        builder.Services.Configure<LogCollectorOptions>(builder.Configuration.GetSection("LogCollector"));

        var configuration = builder.Configuration;
        var healthOptions = configuration.GetSection("Health").Get<HealthOptions>() ?? new();
        var logCollectorOptions = configuration.GetSection("LogCollector").Get<LogCollectorOptions>() ?? new();
        var logStorageOptions = configuration.GetSection("LogStorage").Get<LogStorageOptions>() ?? new();

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
