namespace SecureHomeSystem.Infrastructure;

/// <summary>
/// Represents the outcome of a command executed by <see cref="IProcessRunner" />.
/// </summary>
public sealed record ProcessExecutionResult(int ExitCode, string StandardOutput, string StandardError)
{
    public bool IsSuccess => ExitCode == 0;
}
