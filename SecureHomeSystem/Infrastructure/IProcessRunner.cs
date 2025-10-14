namespace SecureHomeSystem.Infrastructure;

/// <summary>
/// Stellt eine testfreundliche Abstraktion zum Ausführen externer Prozesse bereit.
/// </summary>
public interface IProcessRunner
{
    Task<ProcessExecutionResult> RunAsync(ProcessInvocation invocation, CancellationToken cancellationToken);
}
