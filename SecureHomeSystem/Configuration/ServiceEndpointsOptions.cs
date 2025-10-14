namespace SecureHomeSystem.Configuration;

public sealed class ServiceEndpointsOptions
{
    /// <summary>
    /// Endpunkte und Metadaten für Ollama. Der Worker protokolliert diese Angaben,
    /// damit Moderator:innen in Demos verlässliche URLs nennen können.
    /// </summary>
    public OllamaOptions Ollama { get; set; } = new();

    /// <summary>
    /// OpenWebUI-Endpunktkonfiguration. Werte mit Compose-Overrides abgleichen, damit
    /// Erkennungs-Logs korrekt bleiben.
    /// </summary>
    public OpenWebUiOptions OpenWebUi { get; set; } = new();

    /// <summary>
    /// Stable-Diffusion-Endpunkt sowie Hinweise für Smoke-Tests. Der Worker erfasst
    /// die Einstellungen bereits für spätere Automatisierung.
    /// </summary>
    public StableDiffusionOptions StableDiffusion { get; set; } = new();
}

public sealed class OllamaOptions
{
    /// <summary>
    /// Nutzerfreundlicher Name für Worker-Logs und Dokumentation.
    /// </summary>
    public string DisplayName { get; set; } = "Ollama";

    /// <summary>
    /// Basis-URL, die der Worker beim Stack-Start in den Logs ausgibt.
    /// </summary>
    public string BaseUrl { get; set; } = "http://host.docker.internal:11434";

    /// <summary>
    /// Empfohlenes GPU-Speicherlimit (Anteil). Entspricht <c>OLLAMA_MAX_GPU_MEMORY</c>
    /// in Compose.
    /// </summary>
    public double MaxGpuMemoryFraction { get; set; } = 0.8;

    /// <summary>
    /// HTTP-Pfad für manuelle Health Checks.
    /// </summary>
    public string HealthEndpoint { get; set; } = "/api/tags";
}

public sealed class OpenWebUiOptions
{
    /// <summary>
    /// Anpassbarer Anzeigename für die Weboberfläche, damit Demos das HomeChatGPT-
    /// Branding zeigen können, ohne Compose-Services umzubenennen.
    /// </summary>
    public string DisplayName { get; set; } = "HomeChatGPT";

    /// <summary>
    /// Basis-URL der Weboberfläche. Standard ist das in Compose definierte Port-Binding.
    /// </summary>
    public string BaseUrl { get; set; } = "http://localhost:3003";

    /// <summary>
    /// Zeigt an, ob Authentifizierung erzwungen wird. Spiegelt das Flag <c>OPENWEBUI_AUTH</c>.
    /// </summary>
    public bool RequireAuth { get; set; } = false;

    /// <summary>
    /// Empfohlener API-Endpunkt für skriptbasierte Health Checks.
    /// </summary>
    public string HealthEndpoint { get; set; } = "/api/system/info";
}

public sealed class StableDiffusionOptions
{
    /// <summary>
    /// Freundlicher Name, den die Logs spiegeln, um in Stakeholder-Demos keine
    /// technischen Details offenzulegen.
    /// </summary>
    public string DisplayName { get; set; } = "Stable Diffusion";

    /// <summary>
    /// Basis-URL der AUTOMATIC1111-Oberfläche, wenn das Diffusionsprofil aktiv ist.
    /// </summary>
    public string BaseUrl { get; set; } = "http://localhost:7860";

    /// <summary>
    /// Compose-Profil, das für Stable-Diffusion-Ressourcen aktiviert sein muss.
    /// </summary>
    public string LaunchProfile { get; set; } = "diffusion";

    /// <summary>
    /// Platzhalter-Prompt für Smoke-Tests, praktisch bei moderierten Release-Demos.
    /// </summary>
    public string SmokeTestPrompt { get; set; } = "Erzeuge ein 64x64-Diagnosebild";
}
