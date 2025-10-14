using Microsoft.Extensions.Logging;

namespace SecureHomeSystem.Tests.TestDoubles;

internal sealed class TestLogger<T> : ILogger<T>
{
    private sealed class NullScope : IDisposable
    {
        public static readonly NullScope Instance = new();

        public void Dispose()
        {
        }
    }

    public List<LogEntry> Entries { get; } = new();

    IDisposable ILogger.BeginScope<TState>(TState state)
    {
        return NullScope.Instance;
    }

    bool ILogger.IsEnabled(LogLevel logLevel) => true;

    void ILogger.Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        Entries.Add(new LogEntry(logLevel, formatter(state, exception), exception));
    }

    internal sealed record LogEntry(LogLevel Level, string Message, Exception? Exception);
}
