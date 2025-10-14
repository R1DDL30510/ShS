using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Configuration;
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
    /// Builds the default host, binds configuration sections to options, and exposes
    /// `/health` and `/live` endpoints alongside the orchestrator worker.
    /// </summary>
    /// <param name="args">Command-line arguments forwarded by the hosting infrastructure.</param>
    public static void Main(string[] args)
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

        var healthOptions = builder.Configuration.GetSection("Health").Get<HealthOptions>() ?? new();
        var healthPath = NormalizePath(healthOptions.HealthPath, "/health");
        var livenessPath = NormalizePath(healthOptions.LivenessPath, "/live");

        EnsureLogDirectories(builder.Configuration);

        builder.Services
            .AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy());

        builder.Services.AddSingleton<IProcessRunner, ProcessRunner>();
        builder.Services.AddSingleton<IDockerServiceDetector, DockerServiceDetector>();
        builder.Services.AddHostedService<Worker>();

        var logCollectorOptions = builder.Configuration.GetSection("LogCollector").Get<LogCollectorOptions>();
        if (logCollectorOptions is null || logCollectorOptions.Enabled)
        {
            builder.Services.AddHostedService<ContainerLogCollector>();
        }

        builder.WebHost.ConfigureKestrel(options =>
        {
            options.ListenAnyIP(healthOptions.Port);
        });

        try
        {
            var app = builder.Build();

            app.MapHealthChecks(healthPath);
            app.MapGet(livenessPath, () => Results.Ok(new { status = "Healthy" }));

            app.Run();
        }
        finally
        {
            Log.CloseAndFlush();
        }

        static string NormalizePath(string? candidate, string fallback)
        {
            if (string.IsNullOrWhiteSpace(candidate))
            {
                return fallback;
            }

            return candidate.StartsWith('/') ? candidate : $"/{candidate}";
        }

        static void EnsureLogDirectories(IConfiguration configuration)
        {
            var options = configuration.GetSection("LogStorage").Get<LogStorageOptions>() ?? new();

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
}
