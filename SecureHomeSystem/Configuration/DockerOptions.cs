namespace SecureHomeSystem.Configuration;

public sealed class DockerOptions
{
    /// <summary>
    /// Relativer Pfad zur Compose-Datei, auf die sich der Worker bei Befehlsbeispielen bezieht.
    /// Standardmäßig verweist er auf die Repository-Variante.
    /// </summary>
    public string ComposeFilePath { get; set; } = "docker/compose.yaml";

    /// <summary>
    /// Speicherort der Compose-Umgebungsdatei. Hier passen Operatorinnen und Operatoren
    /// host-spezifische Pfade und Zugangsdaten für Probeläufe an.
    /// </summary>
    public string EnvironmentFile { get; set; } = "docker/.env";

    /// <summary>
    /// Compose-Projektname zum Gruppieren der Container. Stabil halten, damit Screenshots
    /// und Befehlsbeispiele langfristig stimmen.
    /// </summary>
    public string ProjectName { get; set; } = "shs-stack";

    /// <summary>
    /// Profile, die der Worker beim Orchestrieren automatisch aktiviert. Standardmäßig leer,
    /// da die Automatisierung noch nicht verdrahtet ist – die Dokumentation nennt jedoch die Zielwerte.
    /// </summary>
    public string[] AutoStartProfiles { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Erkennungseinstellungen, die Docker-Labels auf logische Dienstenamen abbilden.
    /// </summary>
    public ServiceDetectionOptions Detection { get; set; } = new();
}

public sealed class ServiceDetectionOptions
{
    /// <summary>
    /// Wörterbuch, das logische Dienstkennungen den benötigten Docker-Labels zuordnet.
    /// Bei neuen Services müssen die Werte zu den <c>labels</c> in <c>docker/compose.yaml</c> passen.
    /// </summary>
    public Dictionary<string, string> LabelSelector { get; set; } = new()
    {
        ["open-webui"] = "shs.role=open-webui",
        ["qdrant"] = "shs.role=qdrant",
        ["stable-diffusion"] = "shs.role=stable-diffusion"
    };

    /// <summary>
    /// Kulanzzeit (in Sekunden), in der ein Container beim Start auftauchen darf.
    /// </summary>
    public int StartupTimeoutSeconds { get; set; } = 120;

    /// <summary>
    /// Anzahl der Versuche, bevor der Worker die Erkennung als fehlgeschlagen markiert.
    /// Hilfreich zur Planung der gewünschten Robustheit.
    /// </summary>
    public int RetryCount { get; set; } = 3;
}
