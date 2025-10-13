namespace SecureHomeSystem.Configuration;

public sealed class ResourceSchedulerOptions
{
    public double GpuUtilisationThreshold { get; set; } = 0.5;
    public double GpuMemoryThreshold { get; set; } = 0.8;
    public int PollIntervalSeconds { get; set; } = 5;
    public int QueueBackoffSeconds { get; set; } = 30;
}
