using SecureHomeSystem.Infrastructure;

namespace SecureHomeSystem.Tests.TestDoubles;

internal sealed class FakeProcessRunner : IProcessRunner
{
    private readonly Queue<Func<ProcessInvocation, CancellationToken, Task<ProcessExecutionResult>>> _responses = new();

    public List<ProcessInvocation> Invocations { get; } = new();

    public void EnqueueResult(ProcessExecutionResult result)
    {
        _responses.Enqueue((_, _) => Task.FromResult(result));
    }

    public void Enqueue(Func<ProcessInvocation, CancellationToken, Task<ProcessExecutionResult>> callback)
    {
        _responses.Enqueue(callback);
    }

    public Task<ProcessExecutionResult> RunAsync(ProcessInvocation invocation, CancellationToken cancellationToken)
    {
        Invocations.Add(invocation);

        if (_responses.Count == 0)
        {
            throw new InvalidOperationException("Keine Prozessantwort für den Aufruf konfiguriert.");
        }

        return _responses.Dequeue().Invoke(invocation, cancellationToken);
    }
}
