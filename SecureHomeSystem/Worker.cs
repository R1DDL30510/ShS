using Microsoft.Extensions.Options;
using SecureHomeSystem.Configuration;
using SecureHomeSystem.Services;

namespace SecureHomeSystem
{
    public class Worker : BackgroundService
    {
        private static readonly TimeSpan DetectionInterval = TimeSpan.FromSeconds(30);

        private readonly ILogger<Worker> _logger;
        private readonly IDockerServiceDetector _dockerDetector;
        private readonly ServiceEndpointsOptions _serviceEndpoints;

        public Worker(
            ILogger<Worker> logger,
            IDockerServiceDetector dockerDetector,
            IOptions<ServiceEndpointsOptions> serviceEndpoints)
        {
            _logger = logger;
            _dockerDetector = dockerDetector;
            _serviceEndpoints = serviceEndpoints.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "Starting SecureHomeSystem worker targeting Ollama at {OllamaUrl}, OpenWebUI at {OpenWebUIUrl}, StableDiffusion at {StableDiffusionUrl}",
                _serviceEndpoints.Ollama.Url,
                _serviceEndpoints.OpenWebUI.Url,
                _serviceEndpoints.StableDiffusion.Url);

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
            var services = await _dockerDetector.DetectAsync(cancellationToken).ConfigureAwait(false);

            if (services.Count == 0)
            {
                _logger.LogWarning("No shs.role containers detected");
                return;
            }

            foreach (var service in services)
            {
                var shortId = service.ContainerId.Length > 12 ? service.ContainerId[..12] : service.ContainerId;

                _logger.LogInformation(
                    "Detected {Role} container {ContainerId} | running={Running} | endpoint={Endpoint} | health={Health}",
                    service.Role,
                    shortId,
                    service.IsRunning,
                    service.Address ?? "(none)",
                    service.HealthStatus ?? "unknown");
            }
        }
    }
}
