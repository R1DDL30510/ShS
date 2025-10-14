namespace SecureHomeSystem.Configuration;

/// <summary>
/// Controls where the worker stores structured logs and state files. Defaults
/// assume that docker compose maps a host directory to <c>/logs</c> inside the
/// container.
/// </summary>
public sealed class LogStorageOptions
{
    public string RootPath { get; set; } = "/logs";

    public string WorkerFolderName { get; set; } = "worker";

    public string ServicesFolderName { get; set; } = "services";

    public string StateFolderName { get; set; } = "state";
}
