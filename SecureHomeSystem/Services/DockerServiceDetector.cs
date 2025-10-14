using Microsoft.Extensions.Options;
using SecureHomeSystem.Configuration;
using SecureHomeSystem.Infrastructure;
using SecureHomeSystem.Models;

namespace SecureHomeSystem.Services;

public sealed class DockerServiceDetector : IDockerServiceDetector
{
    private readonly DockerOptions _options;
    private readonly ILogger<DockerServiceDetector> _logger;
    private readonly IProcessRunner _processRunner;

    /// <summary>
    /// Binds configuration and logger dependencies for the detector. All constructor
    /// parameters are resolved by the hosting container; no additional setup is required
    /// when the worker is run via <c>docker compose</c>.
    /// </summary>
    public DockerServiceDetector(
        IOptions<DockerOptions> options,
        ILogger<DockerServiceDetector> logger,
        IProcessRunner processRunner)
    {
        _options = options.Value;
        _logger = logger;
        _processRunner = processRunner;
    }

    /// <summary>
    /// Executes a <c>docker ps</c> command and maps labelled containers to logical services.
    /// The method returns a stable snapshot that the worker logs for observability.
    /// </summary>
    /// <param name="cancellationToken">Token used to abort the CLI invocation during shutdown.</param>
    public async Task<IReadOnlyCollection<DetectedService>> DetectAsync(CancellationToken cancellationToken)
    {
        var detectionOptions = _options.Detection ?? new ServiceDetectionOptions();
        var maxAttempts = Math.Max(1, detectionOptions.RetryCount + 1);
        var deadline = detectionOptions.StartupTimeoutSeconds > 0
            ? DateTimeOffset.UtcNow + TimeSpan.FromSeconds(detectionOptions.StartupTimeoutSeconds)
            : (DateTimeOffset?)null;

        var selectorMap = detectionOptions.LabelSelector;
        var lastSnapshot = new List<DetectedService>();

        for (var attempt = 0; attempt < maxAttempts; attempt++)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("Docker service detection cancelled.");
                return lastSnapshot;
            }

            try
            {
                var invocation = new ProcessInvocation(
                    "docker",
                    "ps --format \"{{.ID}}||{{.Names}}||{{.Image}}||{{.Status}}||{{.Labels}}\"");

                var result = await _processRunner.RunAsync(invocation, cancellationToken).ConfigureAwait(false);

                if (!result.IsSuccess)
                {
                    var stderrMessage = string.IsNullOrWhiteSpace(result.StandardError)
                        ? "(empty)"
                        : result.StandardError.Trim();

                    _logger.LogWarning(
                        "Docker CLI returned non-zero exit code {ExitCode}. stderr: {StdErr} (attempt {Attempt}/{TotalAttempts})",
                        result.ExitCode,
                        stderrMessage,
                        attempt + 1,
                        maxAttempts);
                }
                else
                {
                    var snapshot = MapServices(result.StandardOutput, selectorMap);
                    if (snapshot.Count > 0)
                    {
                        if (attempt > 0)
                        {
                            _logger.LogInformation(
                                "Docker service detection succeeded after {AttemptCount} attempts.",
                                attempt + 1);
                        }

                        return snapshot;
                    }

                    lastSnapshot = snapshot;

                    if (attempt < maxAttempts - 1)
                    {
                        _logger.LogDebug(
                            "Docker CLI returned no labelled services (attempt {Attempt}/{TotalAttempts}); waiting before retry.",
                            attempt + 1,
                            maxAttempts);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Docker service detection cancelled.");
                return lastSnapshot;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to detect docker services.");
                return lastSnapshot;
            }

            var hasFurtherAttempts = attempt < maxAttempts - 1;
            if (!hasFurtherAttempts)
            {
                break;
            }

            if (!deadline.HasValue)
            {
                if (detectionOptions.StartupTimeoutSeconds <= 0)
                {
                    continue;
                }

                var nextDelay = CalculateBackoffDelay(attempt, null);
                if (nextDelay <= TimeSpan.Zero)
                {
                    continue;
                }

                try
                {
                    await Task.Delay(nextDelay, cancellationToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogWarning("Docker service detection cancelled during back-off.");
                    return lastSnapshot;
                }

                continue;
            }

            var remaining = deadline.Value - DateTimeOffset.UtcNow;
            if (remaining <= TimeSpan.Zero)
            {
                _logger.LogWarning(
                    "Docker service detection deadline reached after {ElapsedSeconds} seconds.",
                    detectionOptions.StartupTimeoutSeconds);
                break;
            }

            var delay = CalculateBackoffDelay(attempt, remaining);
            if (delay <= TimeSpan.Zero)
            {
                continue;
            }

            try
            {
                await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Docker service detection cancelled during back-off.");
                return lastSnapshot;
            }
        }

        return lastSnapshot;
    }

    private static List<DetectedService> MapServices(string output, IReadOnlyDictionary<string, string> selectorMap)
    {
        var detected = new List<DetectedService>();

        if (string.IsNullOrWhiteSpace(output))
        {
            return detected;
        }

        foreach (var line in output.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var tokens = line.Split("||", StringSplitOptions.None);
            if (tokens.Length < 5)
            {
                continue;
            }

            var labels = tokens[4]
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(label => label.Trim())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var kvp in selectorMap)
            {
                if (!labels.Contains(kvp.Value))
                {
                    continue;
                }

                detected.Add(new DetectedService
                {
                    Name = kvp.Key,
                    ContainerId = tokens[0],
                    Image = tokens[2],
                    Status = tokens[3],
                    IsRunning = tokens[3].Contains("Up", StringComparison.OrdinalIgnoreCase)
                });

                break;
            }
        }

        return detected;
    }

    private static TimeSpan CalculateBackoffDelay(int attempt, TimeSpan? remaining)
    {
        var backoffMilliseconds = Math.Min(1000, (int)Math.Pow(2, attempt) * 250);
        var delay = TimeSpan.FromMilliseconds(backoffMilliseconds);

        if (!remaining.HasValue)
        {
            return delay;
        }

        if (remaining.Value <= TimeSpan.Zero)
        {
            return TimeSpan.Zero;
        }

        return remaining.Value < delay ? remaining.Value : delay;
    }
}
