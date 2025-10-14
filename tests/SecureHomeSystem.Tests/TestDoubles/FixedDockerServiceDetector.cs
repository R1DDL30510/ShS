using SecureHomeSystem.Models;
using SecureHomeSystem.Services;

namespace SecureHomeSystem.Tests.TestDoubles;

internal sealed class FixedDockerServiceDetector : IDockerServiceDetector
{
    private Func<CancellationToken, Task<IReadOnlyCollection<DetectedService>>> _callback;

    public FixedDockerServiceDetector(IReadOnlyCollection<DetectedService> services)
    {
        _callback = _ => Task.FromResult(services);
    }

    public void SetCallback(Func<CancellationToken, Task<IReadOnlyCollection<DetectedService>>> callback)
    {
        _callback = callback;
    }

    public Task<IReadOnlyCollection<DetectedService>> DetectAsync(CancellationToken cancellationToken)
        => _callback(cancellationToken);
}
