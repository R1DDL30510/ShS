namespace SecureHomeSystem.Models;

public sealed class DetectedService
{
    public string Name { get; init; } = string.Empty;
    public string ContainerId { get; init; } = string.Empty;
    public string Image { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public bool IsRunning { get; init; }
}
