using System.ComponentModel.DataAnnotations;

namespace SecureHomeSystem.Configuration;

/// <summary>
/// Background log harvesting configuration. The collector can be disabled via
/// configuration if another agent is responsible for forwarding container logs.
/// </summary>
public sealed class LogCollectorOptions
{
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Interval between log harvesting passes, in seconds.
    /// </summary>
    [Range(1, int.MaxValue)]
    public int PollIntervalSeconds { get; set; } = 30;

    /// <summary>
    /// Initial lookback window used on startup when no cursor file exists yet.
    /// Value is expressed in minutes.
    /// </summary>
    [Range(1, int.MaxValue)]
    public int InitialLookbackMinutes { get; set; } = 10;

    /// <summary>
    /// Retention configuration applied to per-service log files.
    /// </summary>
    [Required]
    public LogRotationOptions Rotation { get; set; } = new();
}
