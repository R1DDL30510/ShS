namespace SecureHomeSystem.Models;

/// <summary>
/// Unveränderliche Darstellung eines Containers, den der Docker-Detector gefunden hat.
/// Jede Instanz steht für einen Logeintrag, damit Moderatorinnen und Moderatoren den Status
/// ohne CLI-Ausgaben erklären können.
/// </summary>
public sealed class DetectedService
{
    /// <summary>
    /// Logischer Dienstschlüssel (z. B. <c>open-webui</c>) gemäß den Erkennungsoptionen.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Vollständige Docker-Container-ID. Der Worker kürzt sie in Logs auf 12 Zeichen,
    /// behält hier jedoch den kompletten Wert zur Nachvollziehbarkeit.
    /// </summary>
    public string ContainerId { get; init; } = string.Empty;

    /// <summary>
    /// Image-Referenz laut <c>docker ps</c>.
    /// </summary>
    public string Image { get; init; } = string.Empty;

    /// <summary>
    /// Rohstatus aus Docker (z. B. <c>Up 2 minutes</c>).
    /// </summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>
    /// Hilfsboolescher Wert, der angibt, ob der Status "Up" enthält.
    /// </summary>
    public bool IsRunning { get; init; }
}
