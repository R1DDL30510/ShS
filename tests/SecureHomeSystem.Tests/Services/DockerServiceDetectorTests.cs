using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SecureHomeSystem.Configuration;
using SecureHomeSystem.Infrastructure;
using SecureHomeSystem.Services;
using SecureHomeSystem.Tests.TestDoubles;

namespace SecureHomeSystem.Tests.Services;

public sealed class DockerServiceDetectorTests
{
    [Fact]
    [Trait("Category", "Unit")]
    public async Task DetectAsync_ReturnsMappedServicesWhenLabelsMatch()
    {
        var fakeRunner = new FakeProcessRunner();
        var stdout = string.Join(
            Environment.NewLine,
            "abc123||open-webui||repo/open-webui:latest||Up 2 minutes||shs.role=open-webui,foo=bar",
            "def456||stable-diffusion||repo/sd:1.0||Exited (0) 3 seconds ago||shs.role=stable-diffusion",
            "ghi789||ignored||repo/ignored:1.0||Up 5 hours||other=label");
        fakeRunner.EnqueueResult(new ProcessExecutionResult(0, stdout, string.Empty));

        var logger = new TestLogger<DockerServiceDetector>();
        var detector = CreateDetector(fakeRunner, logger);

        var result = await detector.DetectAsync(CancellationToken.None);

        result.Should().HaveCount(2);

        result.Select(service => service.Name).Should().BeEquivalentTo("open-webui", "stable-diffusion");

        var openWebUi = result.Single(service => service.Name == "open-webui");
        openWebUi.ContainerId.Should().Be("abc123");
        openWebUi.Image.Should().Be("repo/open-webui:latest");
        openWebUi.Status.Should().Be("Up 2 minutes");
        openWebUi.IsRunning.Should().BeTrue();

        var stableDiffusion = result.Single(service => service.Name == "stable-diffusion");
        stableDiffusion.IsRunning.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task DetectAsync_ReturnsEmptyAndLogsWarningWhenCommandFails()
    {
        var fakeRunner = new FakeProcessRunner();
        fakeRunner.EnqueueResult(new ProcessExecutionResult(1, string.Empty, "permission denied"));

        var logger = new TestLogger<DockerServiceDetector>();
        var detector = CreateDetector(fakeRunner, logger);

        var result = await detector.DetectAsync(CancellationToken.None);

        result.Should().BeEmpty();
        logger.Entries.Should().ContainSingle(entry =>
            entry.Level == LogLevel.Warning &&
            entry.Message.Contains("non-zero exit code", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task DetectAsync_HandlesCancellationGracefully()
    {
        var fakeRunner = new FakeProcessRunner();
        fakeRunner.Enqueue((_, token) =>
        {
            token.ThrowIfCancellationRequested();
            return Task.FromResult(new ProcessExecutionResult(0, string.Empty, string.Empty));
        });

        var logger = new TestLogger<DockerServiceDetector>();
        var detector = CreateDetector(fakeRunner, logger);

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var result = await detector.DetectAsync(cts.Token);

        result.Should().BeEmpty();
        logger.Entries.Should().Contain(entry =>
            entry.Level == LogLevel.Warning &&
            entry.Message.Contains("cancelled", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task DetectAsync_IgnoresMalformedAndUnlabelledLines()
    {
        var fakeRunner = new FakeProcessRunner();
        var stdout = string.Join(
            Environment.NewLine,
            "too-few-fields",
            "abc||missing||fields",
            "abc123||open-webui||repo/open-webui:latest||Up||foo=bar");
        fakeRunner.EnqueueResult(new ProcessExecutionResult(0, stdout, string.Empty));

        var detector = CreateDetector(fakeRunner);

        var result = await detector.DetectAsync(CancellationToken.None);

        result.Should().BeEmpty();
        fakeRunner.Invocations.Should().ContainSingle();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task DetectAsync_RetriesUntilServicesDetected()
    {
        var fakeRunner = new FakeProcessRunner();
        fakeRunner.EnqueueResult(new ProcessExecutionResult(0, string.Empty, string.Empty));

        var stdout = $"{Guid.NewGuid():N}||open-webui||repo/open-webui:latest||Up 5 seconds||shs.role=open-webui";
        fakeRunner.EnqueueResult(new ProcessExecutionResult(0, stdout, string.Empty));

        var detectionOptions = new ServiceDetectionOptions
        {
            RetryCount = 1,
            StartupTimeoutSeconds = 1
        };

        var detector = CreateDetector(fakeRunner, detectionOptions: detectionOptions);

        var result = await detector.DetectAsync(CancellationToken.None);

        result.Should().ContainSingle(service => service.Name == "open-webui");
        fakeRunner.Invocations.Should().HaveCount(2);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task DetectAsync_HonoursRetryCountWhenNoGracePeriod()
    {
        var fakeRunner = new FakeProcessRunner();
        var detectionOptions = new ServiceDetectionOptions
        {
            RetryCount = 3,
            StartupTimeoutSeconds = 0
        };

        for (var i = 0; i < detectionOptions.RetryCount + 1; i++)
        {
            fakeRunner.EnqueueResult(new ProcessExecutionResult(0, string.Empty, string.Empty));
        }

        var detector = CreateDetector(fakeRunner, detectionOptions: detectionOptions);

        var result = await detector.DetectAsync(CancellationToken.None);

        result.Should().BeEmpty();
        fakeRunner.Invocations.Should().HaveCount(detectionOptions.RetryCount + 1);
    }

    private static DockerServiceDetector CreateDetector(
        IProcessRunner processRunner,
        ILogger<DockerServiceDetector>? logger = null,
        ServiceDetectionOptions? detectionOptions = null)
    {
        var baseOptions = detectionOptions is null
            ? new ServiceDetectionOptions
            {
                LabelSelector = new Dictionary<string, string>
                {
                    ["open-webui"] = "shs.role=open-webui",
                    ["qdrant"] = "shs.role=qdrant",
                    ["stable-diffusion"] = "shs.role=stable-diffusion"
                },
                RetryCount = 0,
                StartupTimeoutSeconds = 0
            }
            : new ServiceDetectionOptions
            {
                LabelSelector = detectionOptions.LabelSelector.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.OrdinalIgnoreCase),
                RetryCount = detectionOptions.RetryCount,
                StartupTimeoutSeconds = detectionOptions.StartupTimeoutSeconds
            };

        var options = Options.Create(new DockerOptions
        {
            Detection = baseOptions
        });

        return new DockerServiceDetector(options, logger ?? new TestLogger<DockerServiceDetector>(), processRunner);
    }
}
