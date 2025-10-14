namespace SecureHomeSystem.Configuration;

/// <summary>
/// Konfiguration für die HTTP-Health-Endpunkte des Workers.
/// </summary>
public sealed class HealthOptions
{
    /// <summary>
    /// TCP-Port für Health- und Liveness-Endpunkte.
    /// </summary>
    public int Port { get; set; } = 5080;

    /// <summary>
    /// Pfad für die formale ASP.NET-Core-Health-Check-Middleware.
    /// </summary>
    public string HealthPath { get; set; } = "/health";

    /// <summary>
    /// Pfad für den leichtgewichtigen Liveness-Endpunkt.
    /// </summary>
    public string LivenessPath { get; set; } = "/live";
}
