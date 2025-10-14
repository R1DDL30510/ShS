using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using SecureHomeSystem.Configuration;
using SecureHomeSystem.Infrastructure;
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
    private readonly IProcessRunner _processRunner;
    private readonly LogCollectorOptions _collectorOptions;
    private readonly LogStorageOptions _storageOptions;
    private readonly LogRotationOptions _rotationOptions;
    private readonly TimeSpan _pollInterval;
    private readonly TimeSpan _initialLookback;
    private readonly string _servicesDirectory;
    private readonly string _stateDirectory;

    public ContainerLogCollector(
        ILogger<ContainerLogCollector> logger,
        IDockerServiceDetector dockerServiceDetector,
        IProcessRunner processRunner,
        IOptions<LogCollectorOptions> collectorOptions,
        IOptions<LogStorageOptions> storageOptions)
    {
        _logger = logger;
        _dockerServiceDetector = dockerServiceDetector;
        _processRunner = processRunner;
        _collectorOptions = collectorOptions.Value;
        _storageOptions = storageOptions.Value;
        _rotationOptions = _collectorOptions.Rotation ?? new();

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

        var runningServices = new List<DetectedService>(services.Count);

        foreach (var service in services)
        {
            if (service.IsRunning)
            {
                runningServices.Add(service);
            }
            else
            {
                _logger.LogDebug(
                    "Service {Service} is unavailable with status {Status}; log collection postponed.",
                    service.Name,
                    service.Status);
            }
        }

        if (runningServices.Count == 0)
        {
            _logger.LogInformation("Detected services are not running; skipping log harvest until they recover.");
            return;
        }

        foreach (var service in runningServices)
        {
            await CollectLogsForServiceAsync(service, cancellationToken).ConfigureAwait(false);
        }
    }

    internal Task RunOnceAsync(CancellationToken cancellationToken)
    {
        if (!_collectorOptions.Enabled)
        {
            _logger.LogInformation("Container log collector disabled via configuration.");
            return Task.CompletedTask;
        }

        Directory.CreateDirectory(_servicesDirectory);
        Directory.CreateDirectory(_stateDirectory);

        return CollectLogsAsync(cancellationToken);
    }

    private async Task CollectLogsForServiceAsync(DetectedService service, CancellationToken cancellationToken)
    {
        var cursorPath = Path.Combine(_stateDirectory, $"{service.Name}.cursor");
        var lastCursor = await ReadCursorAsync(cursorPath, cancellationToken).ConfigureAwait(false);
        var since = CalculateSince(lastCursor);

        var invocation = new ProcessInvocation(
            "docker",
            $"logs --since \"{since:O}\" --timestamps {service.ContainerId}")
        {
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

        try
        {
            var result = await _processRunner.RunAsync(invocation, cancellationToken).ConfigureAwait(false);

            if (!result.IsSuccess)
            {
                _logger.LogWarning(
                    "docker logs returned exit code {ExitCode} for service {Service}. stderr: {StdErr}",
                    result.ExitCode,
                    service.Name,
                    string.IsNullOrWhiteSpace(result.StandardError) ? "(empty)" : result.StandardError.Trim());
            }
            else
            {
                var writeResult = await PersistLogLinesAsync(service, result.StandardOutput, lastCursor, cancellationToken).ConfigureAwait(false);
                if (writeResult.LastCursor.HasValue)
                {
                    lastCursor = writeResult.LastCursor.Value;
                    await WriteCursorAsync(cursorPath, writeResult.LastCursor.Value, cancellationToken).ConfigureAwait(false);
                }

                if (writeResult.AppendedCount > 0)
                {
                    _logger.LogInformation(
                        "Collected {Count} log entries for {Service} (container {Container}).",
                        writeResult.AppendedCount,
                        service.Name,
                        service.ContainerId[..Math.Min(12, service.ContainerId.Length)]);
                }
                else
                {
                    _logger.LogDebug("No new log entries for {Service}.", service.Name);
                }
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
        finally
        {
            EnforceLogRetention(service);
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

        var stream = new FileStream(logPath, FileMode.Append, FileAccess.Write, FileShare.Read, 4096, useAsync: true);
        StreamWriter? writer = null;

        try
        {
            writer = new StreamWriter(stream, new UTF8Encoding(false));

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
        finally
        {
            if (writer is not null)
            {
                await writer.DisposeAsync().ConfigureAwait(false);
            }
            else
            {
                await stream.DisposeAsync().ConfigureAwait(false);
            }
        }
    }

    private void EnforceLogRetention(DetectedService service)
    {
        if (_rotationOptions.MaxFileSizeBytes <= 0 &&
            _rotationOptions.MaxFileAgeDays <= 0 &&
            _rotationOptions.MaxArchiveFiles <= 0)
        {
            return;
        }

        var logPath = Path.Combine(_servicesDirectory, $"{service.Name}.log");
        var directory = Path.GetDirectoryName(logPath);
        if (string.IsNullOrWhiteSpace(directory))
        {
            return;
        }

        try
        {
            var now = DateTimeOffset.UtcNow;

            RotateIfNecessary(service, logPath, now);
            PruneArchivedLogs(service, directory, Path.GetFileNameWithoutExtension(logPath), now);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to enforce log retention for {Service}.", service.Name);
        }
    }

    private void RotateIfNecessary(DetectedService service, string logPath, DateTimeOffset now)
    {
        var fileInfo = new FileInfo(logPath);
        if (!fileInfo.Exists)
        {
            return;
        }

        var reasons = new List<string>();

        if (_rotationOptions.MaxFileSizeBytes > 0 &&
            fileInfo.Length >= _rotationOptions.MaxFileSizeBytes)
        {
            reasons.Add($"size {fileInfo.Length}/{_rotationOptions.MaxFileSizeBytes} bytes");
        }

        if (_rotationOptions.MaxFileAgeDays > 0)
        {
            var age = now - new DateTimeOffset(fileInfo.LastWriteTimeUtc, TimeSpan.Zero);
            if (age >= TimeSpan.FromDays(_rotationOptions.MaxFileAgeDays))
            {
                var formattedAge = age.TotalDays.ToString("F1", CultureInfo.InvariantCulture);
                reasons.Add($"age {formattedAge}d/{_rotationOptions.MaxFileAgeDays}d");
            }
        }

        if (reasons.Count == 0)
        {
            return;
        }

        var directory = fileInfo.DirectoryName;
        if (string.IsNullOrWhiteSpace(directory))
        {
            return;
        }

        var baseName = Path.GetFileNameWithoutExtension(fileInfo.Name);
        var suffix = now.ToString("yyyyMMdd'T'HHmmssfff'Z'", CultureInfo.InvariantCulture);
        var archiveName = $"{baseName}-{suffix}.log";
        var archivePath = Path.Combine(directory, archiveName);
        var attempt = 1;

        while (File.Exists(archivePath))
        {
            archiveName = $"{baseName}-{suffix}-{attempt}.log";
            archivePath = Path.Combine(directory, archiveName);
            attempt++;
        }

        File.Move(fileInfo.FullName, archivePath);

        _logger.LogInformation(
            "Rotated service log for {Service}. Archived as {Archive} ({Reason}).",
            service.Name,
            archiveName,
            string.Join(", ", reasons));
    }

    private void PruneArchivedLogs(DetectedService service, string logDirectory, string baseName, DateTimeOffset now)
    {
        var hasAgeLimit = _rotationOptions.MaxFileAgeDays > 0;
        var hasCountLimit = _rotationOptions.MaxArchiveFiles > 0;

        if (!hasAgeLimit && !hasCountLimit)
        {
            return;
        }

        if (!Directory.Exists(logDirectory))
        {
            return;
        }

        var archives = Directory
            .EnumerateFiles(logDirectory, $"{baseName}-*.log", SearchOption.TopDirectoryOnly)
            .Select(path => new FileInfo(path))
            .Where(file => file.Exists)
            .OrderByDescending(file => file.LastWriteTimeUtc)
            .ToList();

        if (archives.Count == 0)
        {
            return;
        }

        if (hasAgeLimit)
        {
            var cutoff = now - TimeSpan.FromDays(_rotationOptions.MaxFileAgeDays);
            foreach (var archive in archives.ToList())
            {
                var lastWrite = new DateTimeOffset(archive.LastWriteTimeUtc, TimeSpan.Zero);
                if (lastWrite < cutoff)
                {
                    try
                    {
                        archive.Delete();
                        archives.Remove(archive);
                        _logger.LogDebug(
                            "Deleted expired service log archive {Archive} for {Service}.",
                            archive.Name,
                            service.Name);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(
                            ex,
                            "Failed to delete expired log archive {Archive} for {Service}.",
                            archive.FullName,
                            service.Name);
                    }
                }
            }
        }

        archives = archives
            .Where(file => file.Exists)
            .OrderByDescending(file => file.LastWriteTimeUtc)
            .ToList();

        if (hasCountLimit && archives.Count > _rotationOptions.MaxArchiveFiles)
        {
            foreach (var archive in archives
                         .Skip(_rotationOptions.MaxArchiveFiles)
                         .ToList())
            {
                try
                {
                    archive.Delete();
                    _logger.LogDebug(
                        "Deleted excess service log archive {Archive} for {Service}.",
                        archive.Name,
                        service.Name);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(
                        ex,
                        "Failed to delete excess log archive {Archive} for {Service}.",
                        archive.FullName,
                        service.Name);
                }
            }
        }
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
