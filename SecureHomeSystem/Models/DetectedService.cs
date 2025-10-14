namespace SecureHomeSystem.Models;

/// <summary>
/// Immutable representation of a container discovered by the Docker detector. Each
/// instance backs a single log entry so that release presenters can narrate container
/// status without digging through CLI output.
/// </summary>
public sealed class DetectedService
{
    /// <summary>
    /// Logical service key (e.g. <c>open-webui</c>) derived from the detection options.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Friendly name resolved from configuration, surfaced in worker logs for
    /// stakeholder-facing messaging.
    /// </summary>
    public string DisplayName { get; init; } = string.Empty;

    /// <summary>
    /// Full Docker container ID. The worker truncates this value to 12 characters in logs
    /// for readability but retains the full string here for traceability.
    /// </summary>
    public string ContainerId { get; init; } = string.Empty;

    /// <summary>
    /// Image reference reported by <c>docker ps</c>.
    /// </summary>
    public string Image { get; init; } = string.Empty;

    /// <summary>
    /// Raw status string returned by Docker (e.g. <c>Up 2 minutes</c>).
    /// </summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>
    /// Convenience boolean indicating whether the status contains "Up".
    /// </summary>
    public bool IsRunning { get; init; }
}
