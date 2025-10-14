namespace SecureHomeSystem.Configuration;

public sealed class ResourceSchedulerOptions
{
    /// <summary>
    /// Zielwert für die GPU-Auslastung (als Anteil). Dokumentiert die Richtlinie,
    /// auch wenn die Durchsetzung noch aussteht.
    /// </summary>
    public double GpuUtilisationThreshold { get; set; } = 0.5;

    /// <summary>
    /// Bevorzugtes VRAM-Limit als Anteil des Gesamtspeichers.
    /// </summary>
    public double GpuMemoryThreshold { get; set; } = 0.8;

    /// <summary>
    /// Platzhalter für die Abtastrate künftiger Telemetriesammler.
    /// </summary>
    public int PollIntervalSeconds { get; set; } = 5;

    /// <summary>
    /// Backoff-Intervall für Warteschlangenjobs, sobald Scheduling-Automatisierung verfügbar ist.
    /// </summary>
    public int QueueBackoffSeconds { get; set; } = 30;
}
