using System.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using SecureHomeSystem.Configuration;
using SecureHomeSystem.Models;
using SecureHomeSystem.Services;
using Xunit;

namespace SecureHomeSystem.Tests.Services;

public sealed class DockerServiceDetectorTests
{
    [Fact]
    public async Task DetectAsync_ParsesLabelledContainers()
    {
        var options = Options.Create(new DockerOptions());
        var runner = new FakeProcessRunner(new ProcessResult
        {
            ExitCode = 0,
            StandardOutput = string.Join(Environment.NewLine,
            [
                "1234567890ab||shs-stack-open-webui-1||ghcr.io/open-webui/open-webui:v0.3.7||Up 2 minutes||shs.role=open-webui",
                "abcdefabcdef||shs-stack-qdrant-1||qdrant/qdrant:v1.15.4||Up 3 minutes||shs.role=qdrant"
            ])
        });

        var detector = new DockerServiceDetector(options, NullLogger<DockerServiceDetector>.Instance, runner);

        var result = await detector.DetectAsync(CancellationToken.None);

        Assert.Collection(
            result,
            service => AssertService(service, "open-webui", "1234567890ab", "ghcr.io/open-webui/open-webui:v0.3.7", "Up 2 minutes", true),
            service => AssertService(service, "qdrant", "abcdefabcdef", "qdrant/qdrant:v1.15.4", "Up 3 minutes", true));
    }

    [Fact]
    public async Task DetectAsync_IgnoresUnlabelledContainers()
    {
        var options = Options.Create(new DockerOptions());
        var runner = new FakeProcessRunner(new ProcessResult
        {
            ExitCode = 0,
            StandardOutput = "123||unmanaged||image:latest||Up 1 minute||"
        });

        var detector = new DockerServiceDetector(options, NullLogger<DockerServiceDetector>.Instance, runner);

        var result = await detector.DetectAsync(CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task DetectAsync_ReturnsEmptyWhenCliFails()
    {
        var options = Options.Create(new DockerOptions());
        var runner = new FakeProcessRunner(new ProcessResult
        {
            ExitCode = 1,
            StandardError = "permission denied"
        });

        var detector = new DockerServiceDetector(options, NullLogger<DockerServiceDetector>.Instance, runner);

        var result = await detector.DetectAsync(CancellationToken.None);

        Assert.Empty(result);
    }

    private static void AssertService(
        DetectedService service,
        string expectedName,
        string expectedContainerId,
        string expectedImage,
        string expectedStatus,
        bool expectedRunning)
    {
        Assert.Equal(expectedName, service.Name);
        Assert.Equal(expectedContainerId, service.ContainerId);
        Assert.Equal(expectedImage, service.Image);
        Assert.Equal(expectedStatus, service.Status);
        Assert.Equal(expectedRunning, service.IsRunning);
    }

    private sealed class FakeProcessRunner : IProcessRunner
    {
        private readonly ProcessResult _result;

        public FakeProcessRunner(ProcessResult result)
        {
            _result = result;
        }

        public Task<ProcessResult> RunAsync(ProcessStartInfo startInfo, CancellationToken cancellationToken)
        {
            return Task.FromResult(_result);
        }
    }
}
