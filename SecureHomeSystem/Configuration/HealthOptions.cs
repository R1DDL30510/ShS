namespace SecureHomeSystem.Configuration;

/// <summary>
/// Configuration for the worker's HTTP health endpoints.
/// </summary>
public sealed class HealthOptions
{
    /// <summary>
    /// TCP port exposed for the health and liveness endpoints.
    /// </summary>
    public int Port { get; set; } = 5080;

    /// <summary>
    /// Path used by the formal ASP.NET Core health check middleware.
    /// </summary>
    public string HealthPath { get; set; } = "/health";

    /// <summary>
    /// Path used by the lightweight liveness endpoint.
    /// </summary>
    public string LivenessPath { get; set; } = "/live";
}
