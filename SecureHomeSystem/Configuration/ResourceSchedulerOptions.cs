namespace SecureHomeSystem.Configuration;

public sealed class ResourceSchedulerOptions
{
    /// <summary>
    /// Angestrebte GPU-Auslastungsobergrenze als Anteil. Die Werte dokumentieren die
    /// Richtlinie, auch wenn die Durchsetzungslogik noch aussteht.
    /// </summary>
    public double GpuUtilisationThreshold { get; set; } = 0.5;

    /// <summary>
    /// Bevorzugte maximale VRAM-Nutzung als Anteil des verfügbaren GPU-Speichers.
    /// </summary>
    public double GpuMemoryThreshold { get; set; } = 0.8;

    /// <summary>
    /// Platzhalter für die Abtastfrequenz zukünftiger Telemetriesammler.
    /// </summary>
    public int PollIntervalSeconds { get; set; } = 5;

    /// <summary>
    /// Rückzugsintervall für wartende Jobs, sobald die Scheduling-Automatisierung bereitsteht.
    /// </summary>
    public int QueueBackoffSeconds { get; set; } = 30;
}
