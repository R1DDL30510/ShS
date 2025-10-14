using System.Text;

namespace SecureHomeSystem.Infrastructure;

/// <summary>
/// Defines a shell invocation to be executed by <see cref="IProcessRunner" />.
/// The abstraction keeps the worker code testable while still mapping closely
/// to <see cref="System.Diagnostics.ProcessStartInfo" /> semantics.
/// </summary>
public sealed record ProcessInvocation(string FileName, string Arguments)
{
    public bool RedirectStandardOutput { get; init; } = true;

    public bool RedirectStandardError { get; init; } = true;

    public Encoding? StandardOutputEncoding { get; init; }

    public Encoding? StandardErrorEncoding { get; init; }

    public string? WorkingDirectory { get; init; }
}
