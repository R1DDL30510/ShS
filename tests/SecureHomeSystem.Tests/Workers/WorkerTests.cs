using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SecureHomeSystem.Configuration;
using SecureHomeSystem.Models;
using SecureHomeSystem.Tests.TestDoubles;

namespace SecureHomeSystem.Tests.Workers;

public sealed class WorkerTests
{
    [Fact]
    [Trait("Category", "Unit")]
    public async Task DetectOnceAsync_LogsInformationWhenNoServicesDetected()
    {
        var logger = new TestLogger<Worker>();
        var detector = new FixedDockerServiceDetector(Array.Empty<DetectedService>());
        var worker = new Worker(logger, detector, Options.Create(new ServiceEndpointsOptions()));

        await worker.DetectOnceAsync(TestContext.Current.CancellationToken);

        logger.Entries.Should().ContainSingle(entry =>
            entry.Level == LogLevel.Information &&
            entry.Message.Contains("No managed docker services detected.", StringComparison.Ordinal));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task DetectOnceAsync_LogsDiscoveredServicesWithTruncatedIdentifier()
    {
        var logger = new TestLogger<Worker>();
        var detector = new FixedDockerServiceDetector(new[]
        {
            new DetectedService
            {
                Name = "open-webui",
                ContainerId = "abc123456789xyz",
                Image = "repo/open-webui:latest",
                Status = "Up 10 minutes",
                IsRunning = true
            }
        });

        var worker = new Worker(logger, detector, Options.Create(new ServiceEndpointsOptions()));

        await worker.DetectOnceAsync(TestContext.Current.CancellationToken);

        logger.Entries.Should().Contain(entry =>
            entry.Level == LogLevel.Information &&
            entry.Message.Contains("Detected service open-webui", StringComparison.Ordinal) &&
            entry.Message.Contains("abc123456789", StringComparison.Ordinal) &&
            entry.Message.Contains("Up 10 minutes", StringComparison.Ordinal));

        logger.Entries.Should().NotContain(entry =>
            entry.Message.Contains("No managed docker services detected.", StringComparison.Ordinal));
    }
}
