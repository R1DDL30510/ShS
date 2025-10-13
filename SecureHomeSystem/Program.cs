using SecureHomeSystem.Configuration;
using SecureHomeSystem.Services;

namespace SecureHomeSystem
{
    /// <summary>
    /// Entry point for the SecureHomeSystem worker host. The bootstrapper wires up
    /// strongly typed configuration objects and registers hosted services so that
    /// the <see cref="Worker"/> background service can orchestrate managed containers.
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

            builder.Services.AddSingleton<IDockerServiceDetector, DockerServiceDetector>();
            builder.Services.AddHostedService<Worker>();

            var host = builder.Build();
            host.Run();
        }
    }
}
