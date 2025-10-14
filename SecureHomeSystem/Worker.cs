using Microsoft.Extensions.Options;
using SecureHomeSystem.Configuration;
using SecureHomeSystem.Services;

namespace SecureHomeSystem;

public class Worker : BackgroundService
{
    /// <summary>
    /// Pause zwischen den Erkennungsläufen. Das Intervall ist bewusst kurz gewählt,
    /// damit sich Containerzustände in Releasetag-Demos nahezu live zeigen lassen und
    /// gleichzeitig das Produktionslogvolumen im Rahmen bleibt.
    /// </summary>
    private static readonly TimeSpan DetectionInterval = TimeSpan.FromSeconds(30);

    private readonly ILogger<Worker> _logger;
    private readonly IDockerServiceDetector _dockerServiceDetector;
    private readonly ServiceEndpointsOptions _serviceEndpoints;

    /// <summary>
    /// Erstellt eine Worker-Instanz mit über Dependency Injection aufgelösten Abhängigkeiten.
    /// Der Konstruktor puffert den <see cref="ServiceEndpointsOptions"/>-Schnappschuss, da
    /// der Worker lediglich Endpunktmetadaten protokolliert und keine Laufzeitaktualisierung
    /// benötigt.
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
    /// Hauptschleife des Hintergrunddienstes. Beim Start protokolliert der Worker die
    /// konfigurierten Endpunkte, damit Moderator:innen Stakeholder sofort auf bereitgestellte
    /// URLs verweisen können. Anschließend stößt er fortlaufend die Docker-Erkennung an,
    /// bis eine Beendigung angefordert wird, und wartet jeweils <see cref="DetectionInterval"/>.
    /// </summary>
    /// <param name="stoppingToken">Token, das der Host beim Herunterfahren propagiert.</param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "SecureHomeSystem-Worker initialisiert. {OllamaName}: {OllamaUrl}, {WebUiName}: {OpenWebUiUrl}, {StableDiffusionName}: {StableDiffusionUrl}",
            _serviceEndpoints.Ollama.DisplayName,
            _serviceEndpoints.Ollama.BaseUrl,
            _serviceEndpoints.OpenWebUi.DisplayName,
            _serviceEndpoints.OpenWebUi.BaseUrl,
            _serviceEndpoints.StableDiffusion.DisplayName,
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
    /// Logeinträge für jeden verwalteten Dienst. Beobachtbarkeit steht über Mutationen – der
    /// Worker verändert keinen Containerzustand, damit sich Release-Proben sicher in
    /// produktionsnahen Umgebungen durchführen lassen.
    /// </summary>
    /// <param name="cancellationToken">Token, das die Erkennung bei einem Host-Shutdown abbricht.</param>
    private async Task DetectDockerServicesAsync(CancellationToken cancellationToken)
    {
        var services = await _dockerServiceDetector.DetectAsync(cancellationToken).ConfigureAwait(false);

        if (services.Count == 0)
        {
            _logger.LogInformation("Keine verwalteten Docker-Dienste erkannt.");
            return;
        }

        foreach (var service in services)
        {
            var shortId = service.ContainerId.Length > 12 ? service.ContainerId[..12] : service.ContainerId;

            _logger.LogInformation(
                "Dienst erkannt {ServiceName} | Container {ContainerId} | Image {Image} | Status {Status} | Läuft {IsRunning}",
                service.DisplayName,
                shortId,
                service.Image,
                service.Status,
                service.IsRunning);
        }
    }
}
