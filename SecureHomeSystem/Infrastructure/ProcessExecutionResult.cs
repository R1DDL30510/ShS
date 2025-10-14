namespace SecureHomeSystem.Infrastructure;

/// <summary>
/// Repräsentiert das Ergebnis eines Befehls, den
/// <see cref="IProcessRunner" /> ausgeführt hat.
/// </summary>
public sealed record ProcessExecutionResult(int ExitCode, string StandardOutput, string StandardError)
{
    public bool IsSuccess => ExitCode == 0;
}
