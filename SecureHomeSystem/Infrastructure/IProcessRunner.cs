namespace SecureHomeSystem.Infrastructure;

/// <summary>
/// Provides a test-friendly abstraction for executing external processes.
/// </summary>
public interface IProcessRunner
{
    Task<ProcessExecutionResult> RunAsync(ProcessInvocation invocation, CancellationToken cancellationToken);
}
