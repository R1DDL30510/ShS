using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Options;
using SecureHomeSystem.Configuration;
using SecureHomeSystem.Models;

namespace SecureHomeSystem.Services;

public sealed class DockerServiceDetector : IDockerServiceDetector
{
    private readonly DockerOptions _options;
    private readonly ILogger<DockerServiceDetector> _logger;

    /// <summary>
    /// Bindet Konfigurations- und Logger-Abhängigkeiten für den Detektor. Sämtliche
    /// Parameter werden vom Hosting-Container aufgelöst; zusätzliche Einrichtung ist
    /// nicht nötig, wenn der Worker über <c>docker compose</c> läuft.
    /// </summary>
    public DockerServiceDetector(IOptions<DockerOptions> options, ILogger<DockerServiceDetector> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// Führt <c>docker ps</c> aus und ordnet gelabelte Container den logischen Diensten zu.
    /// Die Methode liefert einen stabilen Schnappschuss, den der Worker zur Beobachtbarkeit
    /// protokolliert.
    /// </summary>
    /// <param name="cancellationToken">Token, das den CLI-Aufruf beim Herunterfahren abbricht.</param>
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
            using var process = Process.Start(processStartInfo);
            if (process is null)
            {
                _logger.LogWarning("Docker-CLI-Prozess für die Diensterkennung konnte nicht gestartet werden.");
                return detected;
            }

            var stdoutTask = process.StandardOutput.ReadToEndAsync();
            var stderrTask = process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync(cancellationToken);
            var stdout = await stdoutTask;
            var stderr = await stderrTask;

            if (process.ExitCode != 0)
            {
                _logger.LogWarning("Docker-CLI lieferte Exitcode {ExitCode} ungleich null. stderr: {StdErr}", process.ExitCode, stderr);
                return detected;
            }

            var selectorMap = _options.Detection.LabelSelector;
            if (selectorMap.Count == 0)
            {
                _logger.LogInformation("Keine Docker-Label-Selektoren konfiguriert – Erkennung wird übersprungen.");
                return detected;
            }

            var friendlyNames = _options.Detection.FriendlyNames;

            foreach (var line in stdout.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
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

                    var displayName = friendlyNames.TryGetValue(kvp.Key, out var friendlyName)
                        ? friendlyName
                        : kvp.Key;

                    detected.Add(new DetectedService
                    {
                        Name = kvp.Key,
                        DisplayName = displayName,
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
            _logger.LogWarning("Docker-Diensterkennung abgebrochen.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Docker-Dienste konnten nicht erkannt werden.");
        }

        return detected;
    }
}
