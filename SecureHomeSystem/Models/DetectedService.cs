namespace SecureHomeSystem.Models;

/// <summary>
/// Unveränderliche Darstellung eines Containers, den der Docker-Detektor gefunden hat.
/// Jede Instanz entspricht einem Logeintrag, damit Präsentierende den Containerstatus
/// schildern können, ohne CLI-Ausgaben durchsuchen zu müssen.
/// </summary>
public sealed class DetectedService
{
    /// <summary>
    /// Logischer Dienstschlüssel (z. B. <c>open-webui</c>), abgeleitet aus den Erkennungsoptionen.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Konfigurierter Anzeigename, der in den Worker-Logs für Stakeholder-Kommunikation erscheint.
    /// </summary>
    public string DisplayName { get; init; } = string.Empty;

    /// <summary>
    /// Vollständige Docker-Container-ID. Die Logs kürzen den Wert auf 12 Zeichen,
    /// hier bleibt er aus Gründen der Nachvollziehbarkeit vollständig erhalten.
    /// </summary>
    public string ContainerId { get; init; } = string.Empty;

    /// <summary>
    /// Image-Referenz laut <c>docker ps</c>.
    /// </summary>
    public string Image { get; init; } = string.Empty;

    /// <summary>
    /// Unverarbeiteter Status-String von Docker (z. B. <c>Up 2 minutes</c>).
    /// </summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>
    /// Hilfsbool, das anzeigt, ob der Status den Begriff „Up“ enthält.
    /// </summary>
    public bool IsRunning { get; init; }
}
