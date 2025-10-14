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
/// Einstiegspunkt für den SecureHomeSystem-Worker-Host. Der Bootstrapper konfiguriert
/// stark typisierte Optionen, registriert den Hintergrunddienst und stellt schlanke
/// HTTP-Endpunkte für Health-Monitoring bereit.
/// </summary>
public static class Program
{
    /// <summary>
    /// Baut den Webanwendungs-Host, führt ihn aus und stellt sicher, dass Serilog beim Shutdown seine Puffer leert.
    /// </summary>
    /// <param name="args">Von der Hosting-Infrastruktur weitergeleitete Befehlszeilenargumente.</param>
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
    /// Konfiguriert und baut die ASP.NET-Core-Webanwendung zum Hosten des Workers.
    /// Tests nutzen die Methode, um Service-Registrierungen ohne Kestrel-Start zu prüfen.
    /// </summary>
    /// <param name="args">Von der Hosting-Infrastruktur weitergeleitete Befehlszeilenargumente.</param>
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
        app.MapGet(livenessPath, () => Results.Ok(new { status = "Gesund" }));

        return app;
    }

    /// <summary>
    /// Normalisiert benutzerdefinierte Pfade, indem genau ein führender Slash erzwungen wird.
    /// </summary>
    /// <param name="candidate">Kandidat aus der Konfiguration.</param>
    /// <param name="fallback">Fallback, wenn der Kandidat null oder leer ist.</param>
    internal static string NormalizePath(string? candidate, string fallback)
    {
        if (string.IsNullOrWhiteSpace(candidate))
        {
            return fallback;
        }

        return candidate.StartsWith('/') ? candidate : $"/{candidate}";
    }

    /// <summary>
    /// Stellt sicher, dass die Logverzeichnisse des Workers vor dem Start existieren.
    /// </summary>
    /// <param name="options">Aufgelöste Speicheroptionen.</param>
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
