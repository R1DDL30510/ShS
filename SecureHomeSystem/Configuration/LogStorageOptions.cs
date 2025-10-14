namespace SecureHomeSystem.Configuration;

/// <summary>
/// Steuert, wo der Worker strukturierte Logs und Zustandsdateien ablegt.
/// Standardannahme ist, dass docker compose ein Host-Verzeichnis als <c>/logs</c>
/// in den Container mountet.
/// </summary>
public sealed class LogStorageOptions
{
    public string RootPath { get; set; } = "/logs";

    public string WorkerFolderName { get; set; } = "worker";

    public string ServicesFolderName { get; set; } = "services";

    public string StateFolderName { get; set; } = "state";
}
