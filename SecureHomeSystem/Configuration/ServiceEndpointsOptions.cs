namespace SecureHomeSystem.Configuration;

public sealed class ServiceEndpointsOptions
{
    /// <summary>
    /// Endpunkte und Metadaten für Ollama. Der Worker protokolliert die Angaben,
    /// damit Vortragende in Demos korrekte URLs nennen können.
    /// </summary>
    public OllamaOptions Ollama { get; set; } = new();

    /// <summary>
    /// OpenWebUI-Endpunktkonfiguration. Halte die Werte synchron zu den Compose-Overrides,
    /// damit die Erkennungslogs unverfälscht bleiben.
    /// </summary>
    public OpenWebUiOptions OpenWebUi { get; set; } = new();

    /// <summary>
    /// Stable-Diffusion-Endpunkt und Hinweise für Smoke-Tests. Der Worker erfasst
    /// die Einstellungen bereits für zukünftige Automatisierung.
    /// </summary>
    public StableDiffusionOptions StableDiffusion { get; set; } = new();
}

public sealed class OllamaOptions
{
    /// <summary>
    /// Basis-URL, die der Worker beim Stack-Start in die Logs schreibt.
    /// </summary>
    public string BaseUrl { get; set; } = "http://host.docker.internal:11434";

    /// <summary>
    /// Empfohlenes GPU-Speicherlimit (als Anteil). Entspricht <c>OLLAMA_MAX_GPU_MEMORY</c> in Compose.
    /// </summary>
    public double MaxGpuMemoryFraction { get; set; } = 0.8;

    /// <summary>
    /// HTTP-Pfad für manuelle Health-Checks.
    /// </summary>
    public string HealthEndpoint { get; set; } = "/api/tags";
}

public sealed class OpenWebUiOptions
{
    /// <summary>
    /// Basis-URL der Web-Oberfläche. Standardmäßig entspricht sie dem Compose-Port.
    /// </summary>
    public string BaseUrl { get; set; } = "http://localhost:3003";

    /// <summary>
    /// Gibt an, ob Authentifizierung erzwungen wird. Spiegelt das Flag <c>OPENWEBUI_AUTH</c> wider.
    /// </summary>
    public bool RequireAuth { get; set; } = false;

    /// <summary>
    /// API-Endpunkt, der sich für automatisierte Health-Checks eignet.
    /// </summary>
    public string HealthEndpoint { get; set; } = "/api/system/info";
}

public sealed class StableDiffusionOptions
{
    /// <summary>
    /// Basis-URL der AUTOMATIC1111-Oberfläche, wenn das Diffusionsprofil aktiv ist.
    /// </summary>
    public string BaseUrl { get; set; } = "http://localhost:7860";

    /// <summary>
    /// Compose-Profil, das für das Starten der Stable-Diffusion-Ressourcen benötigt wird.
    /// </summary>
    public string LaunchProfile { get; set; } = "diffusion";

    /// <summary>
    /// Platzhalter-Prompt für Smoke-Tests – praktisch für moderierte Release-Demos.
    /// </summary>
    public string SmokeTestPrompt { get; set; } = "Erzeuge ein 64x64-Diagnosebild";
}
