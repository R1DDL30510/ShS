using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SecureHomeSystem.Configuration;
using SecureHomeSystem.Services;

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

        builder.Services.Configure<DockerOptions>(builder.Configuration.GetSection("Docker"));
        builder.Services.Configure<ServiceEndpointsOptions>(builder.Configuration.GetSection("Services"));
        builder.Services.Configure<ResourceSchedulerOptions>(builder.Configuration.GetSection("ResourceScheduler"));
        builder.Services.Configure<HealthOptions>(builder.Configuration.GetSection("Health"));

        var healthOptions = builder.Configuration.GetSection("Health").Get<HealthOptions>() ?? new();
        var healthPath = NormalizePath(healthOptions.HealthPath, "/health");
        var livenessPath = NormalizePath(healthOptions.LivenessPath, "/live");

        builder.Services
            .AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy());

        builder.Services.AddSingleton<IDockerServiceDetector, DockerServiceDetector>();
        builder.Services.AddHostedService<Worker>();

        builder.WebHost.ConfigureKestrel(options =>
        {
            options.ListenAnyIP(healthOptions.Port);
        });

        var app = builder.Build();

        app.MapHealthChecks(healthPath);
        app.MapGet(livenessPath, () => Results.Ok(new { status = "Healthy" }));

        app.Run();

        static string NormalizePath(string? candidate, string fallback)
        {
            if (string.IsNullOrWhiteSpace(candidate))
            {
                return fallback;
            }

            return candidate.StartsWith('/') ? candidate : $"/{candidate}";
        }
    }
}
