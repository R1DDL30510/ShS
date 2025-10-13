using Microsoft.Extensions.Options;
using SecureHomeSystem.Configuration;
using SecureHomeSystem.Services;

namespace SecureHomeSystem;

public class Worker : BackgroundService
{
    private static readonly TimeSpan DetectionInterval = TimeSpan.FromSeconds(30);

    private readonly ILogger<Worker> _logger;
    private readonly IDockerServiceDetector _dockerServiceDetector;
    private readonly ServiceEndpointsOptions _serviceEndpoints;

    public Worker(
        ILogger<Worker> logger,
        IDockerServiceDetector dockerServiceDetector,
        IOptions<ServiceEndpointsOptions> serviceEndpoints)
    {
        _logger = logger;
        _dockerServiceDetector = dockerServiceDetector;
        _serviceEndpoints = serviceEndpoints.Value;
    }

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
