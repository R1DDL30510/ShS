using System.ComponentModel.DataAnnotations;

namespace SecureHomeSystem.Configuration;

/// <summary>
/// Controls where the worker stores structured logs and state files. Defaults
/// assume that docker compose maps a host directory to <c>/logs</c> inside the
/// container.
/// </summary>
public sealed class LogStorageOptions
{
    /// <summary>
    /// Root directory for all log artefacts created by the worker.
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string RootPath { get; set; } = "/logs";

    /// <summary>
    /// Subdirectory used for worker-specific logs.
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string WorkerFolderName { get; set; } = "worker";

    /// <summary>
    /// Subdirectory that stores service log output.
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string ServicesFolderName { get; set; } = "services";

    /// <summary>
    /// Subdirectory reserved for cursor and state files.
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string StateFolderName { get; set; } = "state";
}
