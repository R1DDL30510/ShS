using SecureHomeSystem.Models;

namespace SecureHomeSystem.Services;

/// <summary>
/// Contract used by the worker to decouple container discovery from background
/// orchestration logic. Implementations should remain side-effect free.
/// </summary>
public interface IDockerServiceDetector
{
    /// <summary>
    /// Returns the set of Docker services currently labelled for orchestration.
    /// </summary>
    Task<IReadOnlyCollection<DetectedService>> DetectAsync(CancellationToken cancellationToken);
}
