using SecureHomeSystem.Models;

namespace SecureHomeSystem.Services;

public interface IDockerServiceDetector
{
    Task<IReadOnlyCollection<DetectedService>> DetectAsync(CancellationToken cancellationToken);
}
