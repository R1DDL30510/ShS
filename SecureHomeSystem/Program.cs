using SecureHomeSystem.Configuration;
using SecureHomeSystem.Services;

namespace SecureHomeSystem
{
    public class Program
    {
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
