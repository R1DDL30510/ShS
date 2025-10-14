namespace SecureHomeSystem.Configuration;

/// <summary>
/// Konfiguration für die Hintergrundsammlung von Logs. Der Collector kann über
/// Konfiguration deaktiviert werden, falls ein anderer Dienst die Container-Logs weiterleitet.
/// </summary>
public sealed class LogCollectorOptions
{
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Intervall zwischen Log-Sammlungsdurchläufen in Sekunden.
    /// </summary>
    public int PollIntervalSeconds { get; set; } = 30;

    /// <summary>
    /// Anfangszeitraum beim Start, sofern noch keine Cursor-Datei existiert.
    /// Wert in Minuten.
    /// </summary>
    public int InitialLookbackMinutes { get; set; } = 10;

    /// <summary>
    /// Aufbewahrungskonfiguration für die Logdateien je Service.
    /// </summary>
    public LogRotationOptions Rotation { get; set; } = new();
}
