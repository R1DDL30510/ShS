using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SecureHomeSystem.Configuration;
using SecureHomeSystem.Services;
using SecureHomeSystem.Tests.Support;

namespace SecureHomeSystem.Tests.Bootstrap;

public sealed class ProgramTests
{
    [Theory]
    [Trait("Category", "Unit")]
    [InlineData(null, "/health", "/health")]
    [InlineData("", "/health", "/health")]
    [InlineData(" ", "/health", "/health")]
    [InlineData("live", "/health", "/live")]
    [InlineData("/ready", "/health", "/ready")]
    public void NormalizePath_ReturnsExpectedValue(string? candidate, string fallback, string expected)
    {
        Program.NormalizePath(candidate, fallback).Should().Be(expected);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void EnsureLogDirectories_CreatesConfiguredStructure()
    {
        using var temp = new TempDirectory();

        var options = new LogStorageOptions
        {
            RootPath = Path.Combine(temp.Path, "logs"),
            WorkerFolderName = "worker",
            ServicesFolderName = "services",
            StateFolderName = "state"
        };

        Program.EnsureLogDirectories(options);

        Directory.Exists(options.RootPath).Should().BeTrue();
        Directory.Exists(Path.Combine(options.RootPath, options.WorkerFolderName)).Should().BeTrue();
        Directory.Exists(Path.Combine(options.RootPath, options.ServicesFolderName)).Should().BeTrue();
        Directory.Exists(Path.Combine(options.RootPath, options.StateFolderName)).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task BuildWebApplication_OmitsLogCollectorWhenDisabled()
    {
        using var temp = new TempDirectory();

        Environment.SetEnvironmentVariable("LogStorage__RootPath", temp.Path);
        Environment.SetEnvironmentVariable("LogCollector__Enabled", "false");

        await using var app = Program.BuildWebApplication(Array.Empty<string>());
        try
        {
            var scopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();
            using var scope = scopeFactory.CreateScope();
            var hostedServices = scope.ServiceProvider.GetServices<IHostedService>().ToList();

            hostedServices.Should().NotContain(service => service is ContainerLogCollector);
        }
        finally
        {
            Environment.SetEnvironmentVariable("LogStorage__RootPath", null);
            Environment.SetEnvironmentVariable("LogCollector__Enabled", null);
        }
    }
}
