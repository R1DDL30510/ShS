namespace SecureHomeSystem.Configuration;

public sealed class ServiceEndpointsOptions
{
    public OllamaOptions Ollama { get; set; } = new();
    public OpenWebUiOptions OpenWebUi { get; set; } = new();
    public StableDiffusionOptions StableDiffusion { get; set; } = new();
}

public sealed class OllamaOptions
{
    public string BaseUrl { get; set; } = "http://host.docker.internal:11434";
    public double MaxGpuMemoryFraction { get; set; } = 0.8;
    public string HealthEndpoint { get; set; } = "/api/tags";
}

public sealed class OpenWebUiOptions
{
    public string BaseUrl { get; set; } = "http://localhost:3000";
    public bool RequireAuth { get; set; }
    public string HealthEndpoint { get; set; } = "/api/system/info";
}

public sealed class StableDiffusionOptions
{
    public string BaseUrl { get; set; } = "http://localhost:7860";
    public string LaunchProfile { get; set; } = "diffusion";
    public string SmokeTestPrompt { get; set; } = "test prompt";
}
