using System.Diagnostics;

namespace SecureHomeSystem.Infrastructure;

/// <summary>
/// Standardimplementierung von <see cref="IProcessRunner" />, die Aufrufe über
/// <see cref="Process" /> ausführt.
/// </summary>
public sealed class ProcessRunner : IProcessRunner
{
    public async Task<ProcessExecutionResult> RunAsync(ProcessInvocation invocation, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(invocation);

        var startInfo = new ProcessStartInfo
        {
            FileName = invocation.FileName,
            Arguments = invocation.Arguments,
            RedirectStandardOutput = invocation.RedirectStandardOutput,
            RedirectStandardError = invocation.RedirectStandardError,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        if (!string.IsNullOrWhiteSpace(invocation.WorkingDirectory))
        {
            startInfo.WorkingDirectory = invocation.WorkingDirectory!;
        }

        if (invocation.StandardOutputEncoding is not null)
        {
            startInfo.StandardOutputEncoding = invocation.StandardOutputEncoding;
        }

        if (invocation.StandardErrorEncoding is not null)
        {
            startInfo.StandardErrorEncoding = invocation.StandardErrorEncoding;
        }

        using var process = Process.Start(startInfo);

        if (process is null)
        {
            throw new InvalidOperationException($"Prozess '{invocation.FileName}' konnte nicht gestartet werden.");
        }

        var stdoutTask = invocation.RedirectStandardOutput
            ? process.StandardOutput.ReadToEndAsync()
            : Task.FromResult(string.Empty);

        var stderrTask = invocation.RedirectStandardError
            ? process.StandardError.ReadToEndAsync()
            : Task.FromResult(string.Empty);

        await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);

        var stdout = await stdoutTask.ConfigureAwait(false);
        var stderr = await stderrTask.ConfigureAwait(false);

        return new ProcessExecutionResult(process.ExitCode, stdout, stderr);
    }
}
