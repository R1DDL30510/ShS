using System.Diagnostics;

namespace SecureHomeSystem.Services;

/// <summary>
/// Abstraction over <see cref="Process"/> execution so Docker detection can be
/// verified in isolation. Wrapping the CLI keeps unit tests deterministic and
/// documents the exact command shape used on release day.
/// </summary>
public interface IProcessRunner
{
    /// <summary>
    /// Executes the supplied <paramref name="startInfo"/> and returns collected
    /// standard streams plus the exit code.
    /// </summary>
    Task<ProcessResult> RunAsync(ProcessStartInfo startInfo, CancellationToken cancellationToken);
}

/// <summary>
/// Value object describing the captured output of a completed process.
/// </summary>
public sealed class ProcessResult
{
    /// <summary>
    /// Gets or sets the exit code reported by the process.
    /// </summary>
    public int ExitCode { get; init; }

    /// <summary>
    /// Gets or sets the text read from standard output.
    /// </summary>
    public string StandardOutput { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the text read from standard error.
    /// </summary>
    public string StandardError { get; init; } = string.Empty;
}
