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
            entry.Message.Contains("Docker-CLI lieferte den Exitcode", StringComparison.OrdinalIgnoreCase));
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
            entry.Message.Contains("Docker-Service-Erkennung abgebrochen", StringComparison.OrdinalIgnoreCase));
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

    private static DockerServiceDetector CreateDetector(IProcessRunner processRunner, ILogger<DockerServiceDetector>? logger = null)
    {
        var options = Options.Create(new DockerOptions());
        return new DockerServiceDetector(options, logger ?? new TestLogger<DockerServiceDetector>(), processRunner);
    }
}
