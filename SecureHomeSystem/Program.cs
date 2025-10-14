using SecureHomeSystem.Configuration;
using SecureHomeSystem.Services;

namespace SecureHomeSystem
{
    /// <summary>
    /// Einstiegspunkt für den SecureHomeSystem-Worker-Host. Das Bootstrapper-Setup
    /// verdrahtet stark typisierte Konfigurationsobjekte und registriert gehostete
    /// Dienste, damit der <see cref="Worker"/> die verwalteten Container orchestrieren kann.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Erstellt den Standard-Host, bindet Konfigurationsabschnitte an Optionsobjekte
        /// und startet den gehosteten Worker-Dienst. Die Methode orientiert sich an
        /// <c>dotnet new worker</c>, erweitert das Grundgerüst jedoch um die spezifischen
        /// Konfigurationen des SecureHomeSystem-Stacks.
        /// </summary>
        /// <param name="args">Kommandozeilenargumente, die die Hosting-Infrastruktur weiterreicht.</param>
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
