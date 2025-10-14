using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SecureHomeSystem.Configuration;
using SecureHomeSystem.Infrastructure;
using SecureHomeSystem.Services;
using SecureHomeSystem.Tests.Support;
using SecureHomeSystem.Tests.TestDoubles;

namespace SecureHomeSystem.Tests.Scenarios;

public sealed class FullStackVerificationTests
{
    [Fact]
    [Trait("Category", "Integration")]
    public async Task FullStackScenario_RunsDetectionAndLogCollection()
    {
        using var temp = new TempDirectory();
        var processRunner = new FakeProcessRunner();

        var detectionOutput = string.Join(
            Environment.NewLine,
            "abc123456789ab||open-webui||repo/open-webui:latest||Up 2 minutes||shs.role=open-webui");

        processRunner.EnqueueResult(new ProcessExecutionResult(0, detectionOutput, string.Empty));
        processRunner.EnqueueResult(new ProcessExecutionResult(0, detectionOutput, string.Empty));

        var baseTime = DateTimeOffset.UtcNow;
        var logsOutput = string.Join(
            Environment.NewLine,
            $"{baseTime.AddSeconds(-2):O} first-message",
            $"{baseTime.AddSeconds(-1):O} second-message");

        processRunner.EnqueueResult(new ProcessExecutionResult(0, logsOutput, string.Empty));

        var detectorLogger = new TestLogger<DockerServiceDetector>();
        var detector = new DockerServiceDetector(
            Options.Create(new DockerOptions()),
            detectorLogger,
            processRunner);

        var workerLogger = new TestLogger<Worker>();
        var worker = new Worker(
            workerLogger,
            detector,
            Options.Create(new ServiceEndpointsOptions()));

        var collectorLogger = new TestLogger<ContainerLogCollector>();
        var collector = new ContainerLogCollector(
            collectorLogger,
            detector,
            processRunner,
            Options.Create(new LogCollectorOptions { Enabled = true }),
            Options.Create(new LogStorageOptions
            {
                RootPath = temp.Path,
                ServicesFolderName = "services",
                StateFolderName = "state"
            }));

        var token = TestContext.Current.CancellationToken;

        await worker.DetectOnceAsync(token);
        await collector.RunOnceAsync(token);

        workerLogger.Entries.Should().Contain(entry =>
            entry.Level == LogLevel.Information &&
            entry.Message.Contains("Service open-webui erkannt", StringComparison.OrdinalIgnoreCase));

        var servicesDirectory = Path.Combine(temp.Path, "services");
        var logPath = Path.Combine(servicesDirectory, "open-webui.log");
        File.Exists(logPath).Should().BeTrue();

        var lines = await File.ReadAllLinesAsync(logPath, token);
        lines.Should().HaveCount(2);

        var records = lines
            .Select(line => JsonSerializer.Deserialize<LogRecord>(line) ?? throw new InvalidOperationException("Logeintrag konnte nicht geparst werden."))
            .ToList();

        records.Select(record => record.Message).Should().BeEquivalentTo("first-message", "second-message");

        var cursorPath = Path.Combine(temp.Path, "state", "open-webui.cursor");
        File.Exists(cursorPath).Should().BeTrue();

        processRunner.Invocations.Should().HaveCount(3);
        processRunner.Invocations[0].FileName.Should().Be("docker");
        processRunner.Invocations[0].Arguments.Should().Contain("ps");
        processRunner.Invocations[2].Arguments.Should().Contain("logs");
    }

    private sealed record LogRecord(DateTimeOffset Timestamp, string Service, string ContainerId, string Message);
}
