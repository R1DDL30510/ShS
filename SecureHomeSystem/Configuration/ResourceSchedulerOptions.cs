namespace SecureHomeSystem.Configuration
{
    public sealed class ResourceSchedulerOptions
    {
        public int GpuUtilisationThreshold { get; set; } = 50;

        public int MemoryHeadroom { get; set; } = 20;

        public TimeSpan PollingInterval { get; set; } = TimeSpan.FromSeconds(10);
    }
}
