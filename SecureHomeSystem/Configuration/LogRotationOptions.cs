namespace SecureHomeSystem.Configuration;

/// <summary>
/// Definiert Aufbewahrungsgrenzen für Servicelogdateien aus dem
/// <see cref="Services.ContainerLogCollector" />. Schwellenwerte sind optional –
/// Null oder negative Werte deaktivieren den jeweiligen Grenzwert.
/// </summary>
public sealed class LogRotationOptions
{
    /// <summary>
    /// Maximale Größe (in Bytes) der aktiven Logdatei, bevor rotiert wird.
    /// </summary>
    public long MaxFileSizeBytes { get; set; } = 10 * 1024 * 1024;

    /// <summary>
    /// Maximales Alter (in Tagen) der aktiven Logdatei, bevor rotiert wird.
    /// </summary>
    public int MaxFileAgeDays { get; set; } = 7;

    /// <summary>
    /// Maximale Anzahl rotierter Archive pro Service. Neuere Dateien bleiben bevorzugt.
    /// Null oder negative Werte deaktivieren das Beschneiden nach Anzahl.
    /// </summary>
    public int MaxArchiveFiles { get; set; } = 5;
}
