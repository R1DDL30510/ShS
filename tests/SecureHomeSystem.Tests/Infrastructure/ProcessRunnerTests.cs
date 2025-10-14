using FluentAssertions;
using SecureHomeSystem.Infrastructure;

namespace SecureHomeSystem.Tests.Infrastructure;

public sealed class ProcessRunnerTests
{
    [Fact]
    [Trait("Category", "Integration")]
    public async Task RunAsync_ReturnsStandardOutputForSuccessfulCommand()
    {
        var runner = new ProcessRunner();
        var invocation = new ProcessInvocation("dotnet", "--version");

        var result = await runner.RunAsync(invocation, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.StandardOutput.Should().NotBeNullOrWhiteSpace();
        result.StandardError.Should().BeNullOrEmpty();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task RunAsync_CapturesErrorOutputForFailedCommand()
    {
        var runner = new ProcessRunner();
        var invocation = new ProcessInvocation("dotnet", "nonexistent-command");

        var result = await runner.RunAsync(invocation, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeFalse();
        result.ExitCode.Should().NotBe(0);
        result.StandardError.Should().NotBeNullOrWhiteSpace();
    }
}
