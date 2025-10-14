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
    /// Bindet Konfigurations- und Loggerabhängigkeiten für den Detector. Sämtliche
    /// Parameter werden vom Hosting-Container aufgelöst; bei Betrieb über
    /// <c>docker compose</c> ist keine weitere Einrichtung nötig.
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
    /// Führt <c>docker ps</c> aus und ordnet gelabelte Container logischen Diensten zu.
    /// Die Methode liefert einen stabilen Snapshot, den der Worker zur Transparenz protokolliert.
    /// </summary>
    /// <param name="cancellationToken">Token zum Abbrechen des CLI-Aufrufs während des Shutdowns.</param>
    public async Task<IReadOnlyCollection<DetectedService>> DetectAsync(CancellationToken cancellationToken)
    {
        var detected = new List<DetectedService>();

        try
        {
            var invocation = new ProcessInvocation(
                "docker",
                "ps --format \"{{.ID}}||{{.Names}}||{{.Image}}||{{.Status}}||{{.Labels}}\"");

            var result = await _processRunner.RunAsync(invocation, cancellationToken).ConfigureAwait(false);

            if (!result.IsSuccess)
            {
                var stderrMessage = string.IsNullOrWhiteSpace(result.StandardError)
                    ? "(leer)"
                    : result.StandardError.Trim();
                _logger.LogWarning(
                    "Docker-CLI lieferte den Exitcode {ExitCode}. stderr: {StdErr}",
                    result.ExitCode,
                    stderrMessage);
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
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Docker-Service-Erkennung abgebrochen.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Docker-Services konnten nicht ermittelt werden.");
        }

        return detected;
    }
}
