using System.ComponentModel.DataAnnotations;

namespace SecureHomeSystem.Configuration;

/// <summary>
/// Configuration for the worker's HTTP health endpoints.
/// </summary>
public sealed class HealthOptions
{
    /// <summary>
    /// TCP port exposed for the health and liveness endpoints.
    /// </summary>
    [Range(1, 65535)]
    public int Port { get; set; } = 5080;

    /// <summary>
    /// Path used by the formal ASP.NET Core health check middleware.
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string HealthPath { get; set; } = "/health";

    /// <summary>
    /// Path used by the lightweight liveness endpoint.
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string LivenessPath { get; set; } = "/live";
}
