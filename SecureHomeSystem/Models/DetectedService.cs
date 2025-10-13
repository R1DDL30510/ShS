namespace SecureHomeSystem.Models
{
    public sealed class DetectedService
    {
        public required string Role { get; init; }

        public required string ContainerId { get; init; }

        public required string Image { get; init; }

        public bool IsRunning { get; init; }

        public string? Address { get; init; }

        public string? HealthStatus { get; init; }
    }
}
