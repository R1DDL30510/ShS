using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SecureHomeSystem.Configuration;
using SecureHomeSystem.Models;

namespace SecureHomeSystem.Services
{
    public sealed class DockerServiceDetector : IDockerServiceDetector, IAsyncDisposable
    {
        private readonly DockerClient _client;
        private readonly ILogger<DockerServiceDetector> _logger;
        private readonly DockerOptions _options;

        public DockerServiceDetector(ILogger<DockerServiceDetector> logger, IOptions<DockerOptions> options)
        {
            _logger = logger;
            _options = options.Value;
            _client = CreateDockerClient();
        }

        public async Task<IReadOnlyCollection<DetectedService>> DetectAsync(CancellationToken cancellationToken)
        {
            var labelFilters = new Dictionary<string, bool>
            {
                ["shs.role"] = true
            };

            if (!string.IsNullOrWhiteSpace(_options.ProjectName))
            {
                labelFilters[$"com.docker.compose.project={_options.ProjectName}"] = true;
            }

            var filters = new Dictionary<string, IDictionary<string, bool>>
            {
                ["label"] = labelFilters
            };

            IList<ContainerListResponse> containers;

            try
            {
                containers = await _client.Containers.ListContainersAsync(new ListContainersParameters
                {
                    All = true,
                    Filters = filters
                }, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to query Docker for shs.role containers");
                return Array.Empty<DetectedService>();
            }

            var discovered = new List<DetectedService>(containers.Count);

            foreach (var container in containers)
            {
                if (!container.Labels.TryGetValue("shs.role", out var role))
                {
                    continue;
                }

                _logger.LogDebug("Discovered container {ContainerId} for role {Role} with status {Status}", container.ID, role, container.Status);

                var endpoint = ResolveEndpoint(container);

                discovered.Add(new DetectedService
                {
                    Role = role,
                    ContainerId = container.ID,
                    Image = container.Image,
                    IsRunning = string.Equals(container.State, "running", StringComparison.OrdinalIgnoreCase),
                    Address = endpoint,
                    HealthStatus = container.Health?.Status
                });
            }

            return discovered;
        }

        public ValueTask DisposeAsync()
        {
            _client.Dispose();
            return ValueTask.CompletedTask;
        }

        private static DockerClient CreateDockerClient()
        {
            // Prefer environment variables (e.g., DOCKER_HOST) but fall back to the local engine.
            if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("DOCKER_HOST")))
            {
                return new DockerClientConfiguration().CreateClient();
            }

            if (OperatingSystem.IsWindows())
            {
                return new DockerClientConfiguration(new Uri("npipe://./pipe/docker_engine")).CreateClient();
            }

            return new DockerClientConfiguration(new Uri("unix:///var/run/docker.sock")).CreateClient();
        }

        private static string? ResolveEndpoint(ContainerListResponse container)
        {
            foreach (var port in container.Ports)
            {
                if (port.PublicPort == 0)
                {
                    continue;
                }

                var host = string.IsNullOrWhiteSpace(port.IP) || port.IP == "0.0.0.0" ? "localhost" : port.IP;
                var scheme = port.Type == "tcp" ? "http" : port.Type;

                return $"{scheme}://{host}:{port.PublicPort}";
            }

            return null;
        }
    }
}
