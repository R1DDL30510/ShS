using System.ComponentModel.DataAnnotations;

namespace SecureHomeSystem.Configuration;

public sealed class ServiceEndpointsOptions
{
    /// <summary>
    /// Endpoints and metadata for Ollama. The worker logs these details so presenters
    /// can cite accurate URLs during walkthroughs.
    /// </summary>
    [Required]
    public OllamaOptions Ollama { get; set; } = new();

    /// <summary>
    /// OpenWebUI endpoint configuration. Keep the values aligned with Compose overrides
    /// so detection logs remain truthful.
    /// </summary>
    [Required]
    public OpenWebUiOptions OpenWebUi { get; set; } = new();

    /// <summary>
    /// Stable Diffusion endpoint and smoke test hints. The worker currently records
    /// these settings for future automation.
    /// </summary>
    [Required]
    public StableDiffusionOptions StableDiffusion { get; set; } = new();
}

public sealed class OllamaOptions
{
    /// <summary>
    /// Base URL surfaced in worker logs when the stack initialises.
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    [Url]
    public string BaseUrl { get; set; } = "http://host.docker.internal:11434";

    /// <summary>
    /// Advisory GPU memory ceiling (fractional). Aligns with <c>OLLAMA_MAX_GPU_MEMORY</c> in Compose.
    /// </summary>
    [Range(0.0, 1.0)]
    public double MaxGpuMemoryFraction { get; set; } = 0.8;

    /// <summary>
    /// HTTP path used by manual health checks.
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    [RegularExpression("^/.*$", ErrorMessage = "Health endpoint paths must start with a forward slash.")]
    public string HealthEndpoint { get; set; } = "/api/tags";
}

public sealed class OpenWebUiOptions
{
    /// <summary>
    /// Base URL for the web UI. Defaults to the Compose port binding.
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    [Url]
    public string BaseUrl { get; set; } = "http://localhost:3000";

    /// <summary>
    /// Indicates if authentication is enforced. Mirrors the <c>OPENWEBUI_AUTH</c> flag.
    /// </summary>
    public bool RequireAuth { get; set; } = true;

    /// <summary>
    /// API endpoint recommended for scripted health checks.
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    [RegularExpression("^/.*$", ErrorMessage = "Health endpoint paths must start with a forward slash.")]
    public string HealthEndpoint { get; set; } = "/api/system/info";
}

public sealed class StableDiffusionOptions
{
    /// <summary>
    /// Base URL for the AUTOMATIC1111 UI when the diffusion profile is enabled.
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    [Url]
    public string BaseUrl { get; set; } = "http://localhost:7860";

    /// <summary>
    /// Compose profile required to launch Stable Diffusion resources.
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string LaunchProfile { get; set; } = "diffusion";

    /// <summary>
    /// Placeholder prompt for smoke tests, handy when narrating release demos.
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string SmokeTestPrompt { get; set; } = "Generate a 64x64 diagnostic image";
}
