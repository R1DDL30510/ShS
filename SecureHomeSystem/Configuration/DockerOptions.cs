namespace SecureHomeSystem.Configuration;

public sealed class DockerOptions
{
    /// <summary>
    /// Relativer Pfad zur Compose-Datei, auf die sich der Worker bei betrieblichen
    /// Hinweisen bezieht. Standardmäßig verweist sie auf die im Repository gepflegte
    /// Stack-Definition.
    /// </summary>
    public string ComposeFilePath { get; set; } = "docker/compose.yaml";

    /// <summary>
    /// Speicherort der Compose-Umgebungsdatei. Betreiber:innen passen hier während
    /// Release-Probeläufen host-spezifische Pfade und Zugangsdaten an.
    /// </summary>
    public string EnvironmentFile { get; set; } = "docker/.env";

    /// <summary>
    /// Compose-Projektname zur Gruppierung der Container. Stabil halten, damit
    /// Screenshots und Befehls-Snippets langfristig korrekt bleiben.
    /// </summary>
    public string ProjectName { get; set; } = "shs-stack";

    /// <summary>
    /// Profile, die der Worker beim Orchestrieren des Stacks automatisch aktiviert.
    /// Standardmäßig ist die Liste leer, weil die Automatisierung noch nicht
    /// verdrahtet ist; die Dokumentation verweist dennoch auf die Zielwerte.
    /// </summary>
    public string[] AutoStartProfiles { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Erkennungseinstellungen, die Docker-Labels logischen Dienstnamen zuordnen.
    /// </summary>
    public ServiceDetectionOptions Detection { get; set; } = new();
}

public sealed class ServiceDetectionOptions
{
    private Dictionary<string, string> _labelSelector = new(StringComparer.OrdinalIgnoreCase)
    {
        ["open-webui"] = "shs.role=open-webui",
        ["qdrant"] = "shs.role=qdrant",
        ["stable-diffusion"] = "shs.role=stable-diffusion"
    };

    private Dictionary<string, string> _friendlyNames = new(StringComparer.OrdinalIgnoreCase)
    {
        ["open-webui"] = "HomeChatGPT",
        ["qdrant"] = "Qdrant",
        ["stable-diffusion"] = "Stable Diffusion"
    };

    /// <summary>
    /// Wörterbuch, das logische Dienstschlüssel mit den erforderlichen Docker-Labels
    /// verbindet. Die Werte müssen zu den in <c>docker/compose.yaml</c> definierten
    /// <c>labels</c> passen, sobald der Stack erweitert wird.
    /// </summary>
    public Dictionary<string, string> LabelSelector
    {
        get => _labelSelector;
        set => _labelSelector = value is null
            ? new(StringComparer.OrdinalIgnoreCase)
            : new(value, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Nutzerfreundliche Servicenamen, die der Worker in Logs ausgibt. Die
    /// Standardwerte halten das HomeChatGPT-Branding für den OpenWebUI-Container,
    /// ohne Compose-Services umzubenennen – so bleiben Upstream-Merges konfliktfrei.
    /// </summary>
    public Dictionary<string, string> FriendlyNames
    {
        get => _friendlyNames;
        set => _friendlyNames = value is null
            ? new(StringComparer.OrdinalIgnoreCase)
            : new(value, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Kulanzfenster (in Sekunden), das ein Container während des Starts erhält.
    /// </summary>
    public int StartupTimeoutSeconds { get; set; } = 120;

    /// <summary>
    /// Anzahl der Versuche, bevor der Worker die Erkennung als fehlgeschlagen markiert.
    /// Hilft bei der Release-Planung, die gewünschte Robustheit der Schleife festzulegen.
    /// </summary>
    public int RetryCount { get; set; } = 3;
}
