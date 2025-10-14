using System.Globalization;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Options;
using SecureHomeSystem.Configuration;
using SecureHomeSystem.Infrastructure;
using SecureHomeSystem.Models;
using SecureHomeSystem.Services;
using SecureHomeSystem.Tests.Support;
using SecureHomeSystem.Tests.TestDoubles;

namespace SecureHomeSystem.Tests.Services;

public sealed class ContainerLogCollectorTests
{
    [Fact]
    [Trait("Category", "Integration")]
    public async Task RunOnceAsync_WritesNewEntriesAndUpdatesCursor()
    {
        using var temp = new TempDirectory();
        var storage = new LogStorageOptions
        {
            RootPath = temp.Path,
            ServicesFolderName = "services",
            StateFolderName = "state"
        };

        var service = new DetectedService
        {
            Name = "open-webui",
            ContainerId = "abc123456789",
            Image = "repo/open-webui:latest",
            Status = "Up 1 minute",
            IsRunning = true
        };

        var detector = new FixedDockerServiceDetector(new[] { service });
        var processRunner = new FakeProcessRunner();

        var firstTimestamp = DateTimeOffset.UtcNow;
        var firstPayload = string.Join(
            Environment.NewLine,
            $"{firstTimestamp.AddSeconds(-5):O} first-message",
            $"{firstTimestamp:O} second-message");
        processRunner.EnqueueResult(new ProcessExecutionResult(0, firstPayload, string.Empty));

        var collector = CreateCollector(detector, processRunner, storage, new LogCollectorOptions { Enabled = true });

        var token = TestContext.Current.CancellationToken;

        await collector.RunOnceAsync(token);

        var servicesDirectory = Path.Combine(storage.RootPath, storage.ServicesFolderName);
        var stateDirectory = Path.Combine(storage.RootPath, storage.StateFolderName);
        var logPath = Path.Combine(servicesDirectory, $"{service.Name}.log");
        var cursorPath = Path.Combine(stateDirectory, $"{service.Name}.cursor");

        File.Exists(logPath).Should().BeTrue();

        var lines = await File.ReadAllLinesAsync(logPath, token);
        lines.Should().HaveCount(2);

        var records = lines.Select(line => JsonSerializer.Deserialize<LogRecord>(line) ?? throw new InvalidOperationException("Failed to parse log record.")).ToList();
        records.Select(record => record.Message).Should().Contain(new[] { "first-message", "second-message" });
        records.Last().Timestamp.Should().BeCloseTo(firstTimestamp, TimeSpan.FromSeconds(1));

        var cursorText = await File.ReadAllTextAsync(cursorPath, token);
        DateTimeOffset.Parse(cursorText, CultureInfo.InvariantCulture).Should().BeCloseTo(firstTimestamp, TimeSpan.FromSeconds(1));

        var secondPayload = string.Join(
            Environment.NewLine,
            $"{firstTimestamp.AddSeconds(-5):O} duplicate-message",
            $"{firstTimestamp.AddSeconds(5):O} third-message");
        processRunner.EnqueueResult(new ProcessExecutionResult(0, secondPayload, string.Empty));

        await collector.RunOnceAsync(token);

        lines = await File.ReadAllLinesAsync(logPath, token);
        lines.Should().HaveCount(3);
        records = lines.Select(line => JsonSerializer.Deserialize<LogRecord>(line) ?? throw new InvalidOperationException("Failed to parse log record on second pass.")).ToList();

        records.Select(record => record.Message).Should().Contain(new[] { "first-message", "second-message", "third-message" });
        records.Should().NotContain(record => record.Message == "duplicate-message");

        cursorText = await File.ReadAllTextAsync(cursorPath, token);
        DateTimeOffset.Parse(cursorText, CultureInfo.InvariantCulture).Should().BeCloseTo(firstTimestamp.AddSeconds(5), TimeSpan.FromSeconds(1));
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task RunOnceAsync_RotatesLogWhenThresholdExceeded()
    {
        using var temp = new TempDirectory();
        var storage = new LogStorageOptions
        {
            RootPath = temp.Path,
            ServicesFolderName = "services",
            StateFolderName = "state"
        };

        var rotation = new LogRotationOptions
        {
            MaxFileSizeBytes = 32,
            MaxFileAgeDays = 1,
            MaxArchiveFiles = 2
        };

        var service = new DetectedService
        {
            Name = "collector",
            ContainerId = "xyz987654321",
            Image = "repo/collector:latest",
            Status = "Up 1 minute",
            IsRunning = true
        };

        var detector = new FixedDockerServiceDetector(new[] { service });
        var processRunner = new FakeProcessRunner();
        processRunner.EnqueueResult(new ProcessExecutionResult(0, string.Empty, string.Empty));

        var collector = CreateCollector(detector, processRunner, storage, new LogCollectorOptions
        {
            Enabled = true,
            Rotation = rotation
        });

        var servicesDirectory = Path.Combine(storage.RootPath, storage.ServicesFolderName);
        Directory.CreateDirectory(servicesDirectory);

        var logPath = Path.Combine(servicesDirectory, $"{service.Name}.log");
        var token = TestContext.Current.CancellationToken;

        await File.WriteAllTextAsync(logPath, new string('a', (int)rotation.MaxFileSizeBytes + 1), token);
        File.SetLastWriteTimeUtc(logPath, DateTime.UtcNow.AddHours(-2));

        var archiveOld = Path.Combine(servicesDirectory, $"{service.Name}-old.log");
        await File.WriteAllTextAsync(archiveOld, "old", token);
        File.SetLastWriteTimeUtc(archiveOld, DateTime.UtcNow.AddDays(-2));

        var archiveRecent = Path.Combine(servicesDirectory, $"{service.Name}-recent.log");
        await File.WriteAllTextAsync(archiveRecent, "recent", token);
        File.SetLastWriteTimeUtc(archiveRecent, DateTime.UtcNow.AddHours(-3));

        var archiveNewest = Path.Combine(servicesDirectory, $"{service.Name}-newest.log");
        await File.WriteAllTextAsync(archiveNewest, "newest", token);
        File.SetLastWriteTimeUtc(archiveNewest, DateTime.UtcNow.AddHours(-1));

        await collector.RunOnceAsync(token);

        File.Exists(logPath).Should().BeFalse();

        var archives = Directory.GetFiles(servicesDirectory, $"{service.Name}-*.log");
        archives.Should().HaveCount(2);
        archives.Should().NotContain(path => path.EndsWith("old.log", StringComparison.OrdinalIgnoreCase));

        var archiveNames = archives.Select(Path.GetFileName).ToList();
        archiveNames.Should().Contain(name => name.StartsWith($"{service.Name}-", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task RunOnceAsync_SkipsWhenDisabled()
    {
        using var temp = new TempDirectory();
        var storage = new LogStorageOptions
        {
            RootPath = temp.Path,
            ServicesFolderName = "services",
            StateFolderName = "state"
        };

        var service = new DetectedService
        {
            Name = "open-webui",
            ContainerId = "abc123456789",
            Image = "repo/open-webui:latest",
            Status = "Up 1 minute",
            IsRunning = true
        };

        var detector = new FixedDockerServiceDetector(new[] { service });
        var runner = new FakeProcessRunner();

        var collector = CreateCollector(
            detector,
            runner,
            storage,
            new LogCollectorOptions
            {
                Enabled = false
            });

        var token = TestContext.Current.CancellationToken;

        await collector.RunOnceAsync(token);

        Directory.Exists(Path.Combine(storage.RootPath, storage.ServicesFolderName)).Should().BeFalse();
        Directory.Exists(Path.Combine(storage.RootPath, storage.StateFolderName)).Should().BeFalse();
        runner.Invocations.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task RunOnceAsync_IgnoresServicesThatAreNotRunning()
    {
        using var temp = new TempDirectory();
        var storage = new LogStorageOptions
        {
            RootPath = temp.Path,
            ServicesFolderName = "services",
            StateFolderName = "state"
        };

        var service = new DetectedService
        {
            Name = "stable-diffusion",
            ContainerId = "stopped123",
            Image = "repo/stable-diffusion:latest",
            Status = "Exited (0) 3 seconds ago",
            IsRunning = false
        };

        var detector = new FixedDockerServiceDetector(new[] { service });
        var runner = new FakeProcessRunner();

        var collector = CreateCollector(
            detector,
            runner,
            storage,
            new LogCollectorOptions
            {
                Enabled = true
            });

        var token = TestContext.Current.CancellationToken;

        await collector.RunOnceAsync(token);

        var servicesDirectory = Path.Combine(storage.RootPath, storage.ServicesFolderName);
        var stateDirectory = Path.Combine(storage.RootPath, storage.StateFolderName);

        Directory.Exists(servicesDirectory).Should().BeTrue();
        Directory.Exists(stateDirectory).Should().BeTrue();
        Directory.GetFiles(servicesDirectory, $"{service.Name}*.log").Should().BeEmpty();
        runner.Invocations.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task RunOnceAsync_CollectsLogsForRunningServicesOnly()
    {
        using var temp = new TempDirectory();
        var storage = new LogStorageOptions
        {
            RootPath = temp.Path,
            ServicesFolderName = "services",
            StateFolderName = "state"
        };

        var runningService = new DetectedService
        {
            Name = "assistant",
            ContainerId = "running123",
            Image = "repo/assistant:latest",
            Status = "Up 3 minutes",
            IsRunning = true
        };

        var stoppedService = new DetectedService
        {
            Name = "database",
            ContainerId = "stopped456",
            Image = "repo/database:latest",
            Status = "Exited (0) 1 minute ago",
            IsRunning = false
        };

        var detector = new FixedDockerServiceDetector(new[] { runningService, stoppedService });
        var runner = new FakeProcessRunner();
        runner.EnqueueResult(new ProcessExecutionResult(0, $"{DateTimeOffset.UtcNow:O} ready", string.Empty));

        var collector = CreateCollector(
            detector,
            runner,
            storage,
            new LogCollectorOptions
            {
                Enabled = true
            });

        var token = TestContext.Current.CancellationToken;

        await collector.RunOnceAsync(token);

        runner.Invocations.Should().ContainSingle(invocation => invocation.Arguments.Contains(runningService.ContainerId));
        runner.Invocations.Should().NotContain(invocation => invocation.Arguments.Contains(stoppedService.ContainerId));

        var servicesDirectory = Path.Combine(storage.RootPath, storage.ServicesFolderName);
        var stoppedLogPath = Path.Combine(servicesDirectory, $"{stoppedService.Name}.log");
        File.Exists(stoppedLogPath).Should().BeFalse();
    }

    private static ContainerLogCollector CreateCollector(
        IDockerServiceDetector detector,
        IProcessRunner processRunner,
        LogStorageOptions storageOptions,
        LogCollectorOptions collectorOptions)
    {
        var logger = new TestLogger<ContainerLogCollector>();
        return new ContainerLogCollector(
            logger,
            detector,
            processRunner,
            Options.Create(collectorOptions),
            Options.Create(storageOptions));
    }

    private sealed record LogRecord(DateTimeOffset Timestamp, string Service, string ContainerId, string Message);
}
