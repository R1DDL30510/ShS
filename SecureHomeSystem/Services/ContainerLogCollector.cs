using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using SecureHomeSystem.Configuration;
using SecureHomeSystem.Models;

namespace SecureHomeSystem.Services;

/// <summary>
/// Harvests logs from labelled docker containers and persists them as JSON lines
/// under <c>/logs/services</c>. A per-service cursor prevents duplicate entries
/// across restarts and enables lightweight diffing during rehearsals.
/// </summary>
public sealed class ContainerLogCollector : BackgroundService
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = false
    };

    private readonly ILogger<ContainerLogCollector> _logger;
    private readonly IDockerServiceDetector _dockerServiceDetector;
    private readonly LogCollectorOptions _collectorOptions;
    private readonly LogStorageOptions _storageOptions;
    private readonly TimeSpan _pollInterval;
    private readonly TimeSpan _initialLookback;
    private readonly string _servicesDirectory;
    private readonly string _stateDirectory;

    public ContainerLogCollector(
        ILogger<ContainerLogCollector> logger,
        IDockerServiceDetector dockerServiceDetector,
        IOptions<LogCollectorOptions> collectorOptions,
        IOptions<LogStorageOptions> storageOptions)
    {
        _logger = logger;
        _dockerServiceDetector = dockerServiceDetector;
        _collectorOptions = collectorOptions.Value;
        _storageOptions = storageOptions.Value;

        var rootPath = _storageOptions.RootPath;
        _servicesDirectory = Path.Combine(rootPath, _storageOptions.ServicesFolderName);
        _stateDirectory = Path.Combine(rootPath, _storageOptions.StateFolderName);

        _pollInterval = TimeSpan.FromSeconds(Math.Max(1, _collectorOptions.PollIntervalSeconds));
        _initialLookback = TimeSpan.FromMinutes(Math.Max(0, _collectorOptions.InitialLookbackMinutes));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_collectorOptions.Enabled)
        {
            _logger.LogInformation("Container log collector disabled via configuration.");
            return;
        }

        Directory.CreateDirectory(_servicesDirectory);
        Directory.CreateDirectory(_stateDirectory);

        _logger.LogInformation(
            "Container log collector started. Interval {Interval}s, lookback {Lookback}min, services directory {ServicesDirectory}",
            _pollInterval.TotalSeconds,
            _initialLookback.TotalMinutes,
            _servicesDirectory);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CollectLogsAsync(stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to harvest docker logs.");
            }

            try
            {
                await Task.Delay(_pollInterval, stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }

    private async Task CollectLogsAsync(CancellationToken cancellationToken)
    {
        var services = await _dockerServiceDetector.DetectAsync(cancellationToken).ConfigureAwait(false);

        if (services.Count == 0)
        {
            _logger.LogDebug("No labelled services discovered for log collection.");
            return;
        }

        foreach (var service in services)
        {
            await CollectLogsForServiceAsync(service, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task CollectLogsForServiceAsync(DetectedService service, CancellationToken cancellationToken)
    {
        var cursorPath = Path.Combine(_stateDirectory, $"{service.Name}.cursor");
        var lastCursor = await ReadCursorAsync(cursorPath, cancellationToken).ConfigureAwait(false);
        var since = CalculateSince(lastCursor);

        var processStartInfo = new ProcessStartInfo
        {
            FileName = "docker",
            Arguments = $"logs --since \"{since:O}\" --timestamps {service.ContainerId}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        try
        {
            using var process = Process.Start(processStartInfo);
            if (process is null)
            {
                _logger.LogWarning("Could not start docker logs process for service {Service}.", service.Name);
                return;
            }

            var stdoutTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
            var stderrTask = process.StandardError.ReadToEndAsync(cancellationToken);

            await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);

            var stdout = await stdoutTask.ConfigureAwait(false);
            var stderr = await stderrTask.ConfigureAwait(false);

            if (process.ExitCode != 0)
            {
                _logger.LogWarning(
                    "docker logs returned exit code {ExitCode} for service {Service}. stderr: {StdErr}",
                    process.ExitCode,
                    service.Name,
                    stderr.Trim());
                return;
            }

            var appended = await PersistLogLinesAsync(service, stdout, lastCursor, cancellationToken).ConfigureAwait(false);
            if (appended.LastCursor.HasValue)
            {
                await WriteCursorAsync(cursorPath, appended.LastCursor.Value, cancellationToken).ConfigureAwait(false);
            }

            if (appended.AppendedCount > 0)
            {
                _logger.LogInformation(
                    "Collected {Count} log entries for {Service} (container {Container}).",
                    appended.AppendedCount,
                    service.Name,
                    service.ContainerId[..Math.Min(12, service.ContainerId.Length)]);
            }
            else
            {
                _logger.LogDebug("No new log entries for {Service}.", service.Name);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to collect logs for service {Service}.", service.Name);
        }
    }

    private async Task<(int AppendedCount, DateTimeOffset? LastCursor)> PersistLogLinesAsync(
        DetectedService service,
        string stdout,
        DateTimeOffset? lastCursor,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(stdout))
        {
            return (0, lastCursor ?? DateTimeOffset.UtcNow);
        }

        var lines = stdout
            .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (lines.Length == 0)
        {
            return (0, lastCursor ?? DateTimeOffset.UtcNow);
        }

        var logPath = Path.Combine(_servicesDirectory, $"{service.Name}.log");

        await using var stream = new FileStream(logPath, FileMode.Append, FileAccess.Write, FileShare.Read, 4096, useAsync: true);
        await using var writer = new StreamWriter(stream, new UTF8Encoding(false));

        var appended = 0;
        var maxTimestamp = lastCursor;

        foreach (var line in lines)
        {
            var separatorIndex = line.IndexOf(' ');
            if (separatorIndex <= 0)
            {
                continue;
            }

            var timestampSegment = line[..separatorIndex];
            if (!DateTimeOffset.TryParse(timestampSegment, out var timestamp))
            {
                continue;
            }

            if (lastCursor.HasValue && timestamp <= lastCursor.Value)
            {
                continue;
            }

            var message = line[(separatorIndex + 1)..];

            var record = new ContainerLogRecord
            {
                Timestamp = timestamp,
                Service = service.Name,
                ContainerId = service.ContainerId,
                Message = message
            };

            var json = JsonSerializer.Serialize(record, SerializerOptions);
            await writer.WriteLineAsync(json.AsMemory(), cancellationToken).ConfigureAwait(false);

            appended++;
            if (!maxTimestamp.HasValue || timestamp > maxTimestamp.Value)
            {
                maxTimestamp = timestamp;
            }
        }

        await writer.FlushAsync().ConfigureAwait(false);

        return (appended, maxTimestamp ?? lastCursor ?? DateTimeOffset.UtcNow);
    }

    private DateTimeOffset CalculateSince(DateTimeOffset? lastCursor)
    {
        if (lastCursor.HasValue)
        {
            return lastCursor.Value.AddMilliseconds(-10);
        }

        var lookbackBaseline = DateTimeOffset.UtcNow;

        return _initialLookback > TimeSpan.Zero
            ? lookbackBaseline - _initialLookback
            : lookbackBaseline;
    }

    private static async Task<DateTimeOffset?> ReadCursorAsync(string cursorPath, CancellationToken cancellationToken)
    {
        if (!File.Exists(cursorPath))
        {
            return null;
        }

        var content = await File.ReadAllTextAsync(cursorPath, cancellationToken).ConfigureAwait(false);

        return DateTimeOffset.TryParse(content, out var timestamp)
            ? timestamp
            : null;
    }

    private static Task WriteCursorAsync(string cursorPath, DateTimeOffset value, CancellationToken cancellationToken)
        => File.WriteAllTextAsync(cursorPath, value.ToString("O"), cancellationToken);

    private sealed record ContainerLogRecord
    {
        public DateTimeOffset Timestamp { get; init; }

        public string Service { get; init; } = string.Empty;

        public string ContainerId { get; init; } = string.Empty;

        public string Message { get; init; } = string.Empty;
    }
}
