using SecureHomeSystem.Models;

namespace SecureHomeSystem.Services;

/// <summary>
/// Vertrag, der die Container-Erkennung von der Orchestrierungslogik entkoppelt.
/// Implementierungen sollen nebenwirkungsfrei bleiben.
/// </summary>
public interface IDockerServiceDetector
{
    /// <summary>
    /// Gibt die Menge aktuell gelabelter Docker-Services für die Orchestrierung zurück.
    /// </summary>
    Task<IReadOnlyCollection<DetectedService>> DetectAsync(CancellationToken cancellationToken);
}
