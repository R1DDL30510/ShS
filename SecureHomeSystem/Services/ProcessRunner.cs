using System.Diagnostics;

namespace SecureHomeSystem.Services;

/// <summary>
/// Default implementation of <see cref="IProcessRunner"/> that shells out to the
/// operating system. The logic centralises stream handling so the detector and its
/// tests can focus on parsing behaviour.
/// </summary>
public sealed class ProcessRunner : IProcessRunner
{
    /// <inheritdoc />
    public async Task<ProcessResult> RunAsync(ProcessStartInfo startInfo, CancellationToken cancellationToken)
    {
        using var process = Process.Start(startInfo);

        if (process is null)
        {
            throw new InvalidOperationException("Unable to start process using the provided start info.");
        }

        var stdoutTask = process.StandardOutput.ReadToEndAsync();
        var stderrTask = process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);

        var stdout = await stdoutTask.ConfigureAwait(false);
        var stderr = await stderrTask.ConfigureAwait(false);

        return new ProcessResult
        {
            ExitCode = process.ExitCode,
            StandardOutput = stdout,
            StandardError = stderr
        };
    }
}
