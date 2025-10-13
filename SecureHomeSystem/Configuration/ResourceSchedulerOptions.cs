namespace SecureHomeSystem.Configuration;

/// <summary>
/// Captures aspirational GPU scheduling targets so operations teams can rehearse
/// capacity conversations while automation is under construction. The numbers are
/// still advisory, but surfacing them keeps the release script grounded in data.
/// </summary>
public sealed class ResourceSchedulerOptions
{
    /// <summary>
    /// Target GPU utilisation ceiling expressed as a fraction. These values document
    /// the policy intent even though enforcement logic is still pending.
    /// </summary>
    public double GpuUtilisationThreshold { get; set; } = 0.5;

    /// <summary>
    /// Preferred maximum VRAM consumption as a fraction of total GPU memory.
    /// </summary>
    public double GpuMemoryThreshold { get; set; } = 0.8;

    /// <summary>
    /// Placeholder sampling cadence for future telemetry collectors.
    /// </summary>
    public int PollIntervalSeconds { get; set; } = 5;

    /// <summary>
    /// Backoff interval intended for queued jobs once scheduling automation ships.
    /// </summary>
    public int QueueBackoffSeconds { get; set; } = 30;
}
