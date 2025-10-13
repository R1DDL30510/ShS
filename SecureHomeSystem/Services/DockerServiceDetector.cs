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

    public DockerServiceDetector(IOptions<DockerOptions> options, ILogger<DockerServiceDetector> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

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
                _logger.LogWarning("Unable to start docker CLI process for service detection.");
                return detected;
            }

            var stdoutTask = process.StandardOutput.ReadToEndAsync();
            var stderrTask = process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync(cancellationToken);
            var stdout = await stdoutTask;
            var stderr = await stderrTask;

            if (process.ExitCode != 0)
            {
                _logger.LogWarning("Docker CLI returned non-zero exit code {ExitCode}. stderr: {StdErr}", process.ExitCode, stderr);
                return detected;
            }

            var selectorMap = _options.Detection.LabelSelector;

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
            _logger.LogWarning("Docker service detection cancelled.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to detect docker services.");
        }

        return detected;
    }
}
