using System.Text;

namespace SecureHomeSystem.Infrastructure;

/// <summary>
/// Definiert einen Shell-Aufruf, der von <see cref="IProcessRunner" />
/// ausgeführt wird. Die Abstraktion hält den Worker testbar und orientiert
/// sich eng an den Semantiken von
/// <see cref="System.Diagnostics.ProcessStartInfo" />.
/// </summary>
public sealed record ProcessInvocation(string FileName, string Arguments)
{
    public bool RedirectStandardOutput { get; init; } = true;

    public bool RedirectStandardError { get; init; } = true;

    public Encoding? StandardOutputEncoding { get; init; }

    public Encoding? StandardErrorEncoding { get; init; }

    public string? WorkingDirectory { get; init; }
}
