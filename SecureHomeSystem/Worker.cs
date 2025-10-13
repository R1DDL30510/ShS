using Microsoft.Extensions.Options;
using SecureHomeSystem.Configuration;
using SecureHomeSystem.Services;

namespace SecureHomeSystem;

/// <summary>
/// Hosted service that coordinates Docker discovery and narrates stack state
/// through structured log messages. Each method doubles as a reference script for
/// release demos so presenters can describe what happens between detection passes
/// without reading source code on the fly.
/// </summary>
public class Worker : BackgroundService
{
    /// <summary>
    /// Delay between discovery passes. The interval is intentionally short so that
    /// release-day demos surface container state changes almost immediately while
    /// remaining conservative enough for production logging volumes.
    /// </summary>
    private static readonly TimeSpan DetectionInterval = TimeSpan.FromSeconds(30);

    private readonly ILogger<Worker> _logger;
    private readonly IDockerServiceDetector _dockerServiceDetector;
    private readonly ServiceEndpointsOptions _serviceEndpoints;

    /// <summary>
    /// Creates a worker instance with dependencies resolved through DI. The constructor
    /// caches the <see cref="ServiceEndpointsOptions"/> snapshot because the worker only
    /// logs endpoint metadata and does not require runtime refreshes.
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
    /// Main execution loop for the background service. On startup the worker records
    /// configured endpoints so presenters can quickly point stakeholders to deployed
    /// URLs. It then repeatedly triggers Docker discovery until cancellation is
    /// requested, spacing each pass by <see cref="DetectionInterval"/>.
    /// </summary>
    /// <param name="stoppingToken">Token propagated by the host during shutdown.</param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "SecureHomeSystem worker initialised. Ollama: {OllamaUrl}, OpenWebUI: {OpenWebUiUrl}, StableDiffusion: {StableDiffusionUrl}",
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
    /// Invokes the injected <see cref="IDockerServiceDetector"/> and emits structured log
    /// entries for each managed service. The method favours observability over mutation;
    /// it never alters container state so that release rehearsals can be safely run on
    /// production-like environments.
    /// </summary>
    /// <param name="cancellationToken">Token used to abort detection when the host shuts down.</param>
    private async Task DetectDockerServicesAsync(CancellationToken cancellationToken)
    {
        var services = await _dockerServiceDetector.DetectAsync(cancellationToken).ConfigureAwait(false);

        if (services.Count == 0)
        {
            _logger.LogInformation("No managed docker services detected.");
            return;
        }

        foreach (var service in services)
        {
            var shortId = service.ContainerId.Length > 12 ? service.ContainerId[..12] : service.ContainerId;

            _logger.LogInformation(
                "Detected service {ServiceName} | Container {ContainerId} | Image {Image} | Status {Status} | Running {IsRunning}",
                service.Name,
                shortId,
                service.Image,
                service.Status,
                service.IsRunning);
        }
    }
}
