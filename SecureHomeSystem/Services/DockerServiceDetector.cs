using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Options;
using SecureHomeSystem.Configuration;
using SecureHomeSystem.Models;

namespace SecureHomeSystem.Services;

/// <summary>
/// Executes Docker CLI commands and projects labelled containers into
/// <see cref="DetectedService"/> records. The implementation favours transparency
/// and testability so release rehearsals can be backed by automated verification.
/// </summary>
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
        var detected = new List<DetectedService>();

        var processStartInfo = new ProcessStartInfo
        {
            FileName = "docker",
            Arguments = "ps --format \"{{.ID}}||{{.Names}}||{{.Image}}||{{.Status}}||{{.Labels}}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        try
        {
            var result = await _processRunner.RunAsync(processStartInfo, cancellationToken).ConfigureAwait(false);

            if (result.ExitCode != 0)
            {
                _logger.LogWarning(
                    "Docker CLI returned non-zero exit code {ExitCode}. stderr: {StdErr}",
                    result.ExitCode,
                    result.StandardError);
                return detected;
            }

            var selectorMap = _options.Detection.LabelSelector;

            foreach (var line in result.StandardOutput.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
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
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Unable to start docker CLI process for service detection.");
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Docker service detection cancelled.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to detect docker services.");
        }

        return detected;
    }
}
