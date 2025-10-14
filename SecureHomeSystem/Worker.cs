using Microsoft.Extensions.Options;
using SecureHomeSystem.Configuration;
using SecureHomeSystem.Services;

namespace SecureHomeSystem;

public class Worker : BackgroundService
{
    /// <summary>
    /// Wartezeit zwischen Erkennungsdurchläufen. Kurz genug für Release-Demos,
    /// um Änderungen nahezu live zu zeigen, aber moderat für Produktions-Logmengen.
    /// </summary>
    private static readonly TimeSpan DetectionInterval = TimeSpan.FromSeconds(30);

    private readonly ILogger<Worker> _logger;
    private readonly IDockerServiceDetector _dockerServiceDetector;
    private readonly ServiceEndpointsOptions _serviceEndpoints;

    /// <summary>
    /// Erstellt eine Worker-Instanz mit per DI aufgelösten Abhängigkeiten. Der Konstruktor
    /// cached den <see cref="ServiceEndpointsOptions"/>-Snapshot, da nur Metadaten geloggt
    /// werden und keine Laufzeitaktualisierung nötig ist.
    /// </summary>
    public Worker(
        ILogger<Worker> logger,
        IDockerServiceDetector dockerServiceDetector,
        IOptions<ServiceEndpointsOptions> serviceEndpoints)
    {
        _logger = logger;
        _dockerServiceDetector = dockerServiceDetector;
        _serviceEndpoints = serviceEndpoints.Value;
    }

    /// <summary>
    /// Hauptexekutionsschleife des BackgroundService. Beim Start protokolliert der Worker
    /// die konfigurierten Endpunkte für schnelle Hinweise an Stakeholder und führt dann
    /// wiederholt die Docker-Erkennung aus, getrennt durch das
    /// <see cref="DetectionInterval"/>.
    /// </summary>
    /// <param name="stoppingToken">Vom Host beim Shutdown propagiertes Token.</param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "SecureHomeSystem-Worker initialisiert. Ollama: {OllamaUrl}, OpenWebUI: {OpenWebUiUrl}, StableDiffusion: {StableDiffusionUrl}",
            _serviceEndpoints.Ollama.BaseUrl,
            _serviceEndpoints.OpenWebUi.BaseUrl,
            _serviceEndpoints.StableDiffusion.BaseUrl);

        while (!stoppingToken.IsCancellationRequested)
        {
            await DetectDockerServicesAsync(stoppingToken).ConfigureAwait(false);

            try
            {
                await Task.Delay(DetectionInterval, stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }

    /// <summary>
    /// Ruft den injizierten <see cref="IDockerServiceDetector"/> auf und schreibt strukturierte
    /// Logs für jeden verwalteten Service. Beobachtbarkeit steht im Vordergrund – der Zustand der
    /// Container bleibt unangetastet, damit Probeläufe auf produktionsnahen Umgebungen sicher sind.
    /// </summary>
    /// <param name="cancellationToken">Token zum Abbruch bei Host-Shutdown.</param>
    internal Task DetectOnceAsync(CancellationToken cancellationToken)
        => DetectDockerServicesAsync(cancellationToken);

    private async Task DetectDockerServicesAsync(CancellationToken cancellationToken)
    {
        var services = await _dockerServiceDetector.DetectAsync(cancellationToken).ConfigureAwait(false);

        if (services.Count == 0)
        {
            _logger.LogInformation("Keine verwalteten Docker-Services gefunden.");
            return;
        }

        foreach (var service in services)
        {
            var shortId = service.ContainerId.Length > 12 ? service.ContainerId[..12] : service.ContainerId;

            _logger.LogInformation(
                "Service {ServiceName} erkannt | Container {ContainerId} | Image {Image} | Status {Status} | Läuft {IsRunning}",
                service.Name,
                shortId,
                service.Image,
                service.Status,
                service.IsRunning);
        }
    }
}
