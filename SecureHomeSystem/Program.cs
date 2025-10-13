using SecureHomeSystem.Configuration;
using SecureHomeSystem.Services;

namespace SecureHomeSystem
{
    /// <summary>
    /// Application bootstrapper responsible for constructing and running the
    /// SecureHomeSystem worker host. The class is intentionally lightweight so the
    /// release runbook can mirror the runtime wiring one-to-one when narrating how
    /// configuration binds into services.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Builds the default host, binds configuration sections to options, and
        /// starts the hosted worker service. This method mirrors the setup carried
        /// out by <c>dotnet new worker</c> but adds the bespoke configuration objects
        /// required by the SecureHomeSystem stack.
        /// </summary>
        /// <param name="args">Command-line arguments forwarded by the hosting infrastructure.</param>
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

            builder.Services.Configure<DockerOptions>(builder.Configuration.GetSection("Docker"));
            builder.Services.Configure<ServiceEndpointsOptions>(builder.Configuration.GetSection("Services"));
            builder.Services.Configure<ResourceSchedulerOptions>(builder.Configuration.GetSection("ResourceScheduler"));

            builder.Services.AddSingleton<IProcessRunner, ProcessRunner>();
            builder.Services.AddSingleton<IDockerServiceDetector, DockerServiceDetector>();
            builder.Services.AddHostedService<Worker>();

            var host = builder.Build();
            host.Run();
        }
    }
}
