using SecureHomeSystem.Models;

namespace SecureHomeSystem.Services;

/// <summary>
/// Vertrag, den der Worker nutzt, um Containererkennung von der Orchestrierungslogik
/// im Hintergrund zu entkoppeln. Implementierungen sollten nebenwirkungsfrei bleiben.
/// </summary>
public interface IDockerServiceDetector
{
    /// <summary>
    /// Liefert die Menge der Docker-Dienste zurück, die aktuell für die Orchestrierung gelabelt sind.
    /// </summary>
    Task<IReadOnlyCollection<DetectedService>> DetectAsync(CancellationToken cancellationToken);
}
