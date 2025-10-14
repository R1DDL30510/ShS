# Konfigurationsreferenz

Der Worker lädt die Konfiguration aus `SecureHomeSystem/appsettings.json`, wobei Überschreibungen über Umgebungsvariablen oder zusätzliche JSON-Dateien wie `appsettings.Development.json` angewendet werden. Die folgenden Abschnitte listen die unterstützten Schlüssel und ihre Standardwerte in diesem Repository auf.

## `Docker`
- `ComposeFilePath` (`string`): Pfad zur Compose-Datei. Standard: `docker/compose.yaml`.
- `EnvironmentFile` (`string`): Relativer Pfad zur Compose-`.env`-Datei. Standard: `docker/.env`.
- `ProjectName` (`string`): Compose-Projektname, der bei CLI-Befehlen verwendet wird. Standard: `shs-stack`.
- `AutoStartProfiles` (`string[]`): Compose-Profile, die der Worker zu starten versucht (Repository-Standard: `["worker"]`, Entwicklungs-Override fügt `"diffusion"` hinzu).
- `Detection` (`ServiceDetectionOptions`): Verschachtelte Einstellungen, die steuern, wie Docker-Container zugeordnet werden.

### `Docker:Detection`
- `LabelSelector` (`Dictionary<string,string>`): Erwartete Docker-Label-Filter, nach logischem Dienstnamen indiziert. Standardwerte ordnen den Worker den in `docker/compose.yaml` gesetzten `shs.role`-Labels zu (`open-webui`, `qdrant`, `stable-diffusion`). Schlüssel werden ohne Beachtung der Groß-/Kleinschreibung verglichen; ein leerer Satz deaktiviert die automatische Erkennung vollständig.
- `FriendlyNames` (`Dictionary<string,string>`): Optionale Anzeigenamen, die der Worker in Logs verwendet. Standardmäßig wird `open-webui` als `HomeChatGPT` ausgegeben, ohne Compose-Services umzubenennen. Die Schlüssel sind ebenfalls case-insensitiv; wenn der Block leer bleibt, protokolliert der Worker wieder die internen Servicenamen.
- `StartupTimeoutSeconds` (`int`): Zeitfenster (Sekunden), in dem ein Dienst erscheinen muss, bevor die Erkennung einen Timeout meldet. Standard: `120`.
- `RetryCount` (`int`): Anzahl der Erkennungsversuche, bevor eskaliert wird. Standard: `3`.

## `Services`
### `Services:Ollama`
- `DisplayName` (`string`): Anzeigename für Logs und Demos. Standard: `Ollama`.
- `BaseUrl` (`string`): Endpunkt, der für Worker-Protokolle verwendet wird. Repository-Standard: `http://host.docker.internal:11434`.
- `MaxGpuMemoryFraction` (`double`): Gewünschtes VRAM-Limit, das Operatoren und Downstream-Tools kommuniziert wird. Standard: `0.8` (80 %).
- `HealthEndpoint` (`string`): Pfad für Health Checks. Standard: `/api/tags`.

### `Services:OpenWebUi`
- `DisplayName` (`string`): Branding-Name für die Weboberfläche. Standard: `HomeChatGPT` (OpenWebUI).
- `BaseUrl` (`string`): URL, die der Worker ankündigt. Repository-Standard: `http://localhost:3003` (entspricht dem Compose-Port-Binding).
- `RequireAuth` (`bool`): Spiegelt das Authentifizierungsflag wider, das dem HomeChatGPT-/OpenWebUI-Container übergeben wird. Standard: `false`.
- `HealthEndpoint` (`string`): Pfad für manuelle Health Checks. Standard: `/api/system/info`.

### `Services:StableDiffusion`
- `DisplayName` (`string`): Anzeigename für Stakeholder-Kommunikation. Standard: `Stable Diffusion`.
- `BaseUrl` (`string`): Endpunkt der Stable-Diffusion-Oberfläche. Repository-Standard: `http://localhost:7860`.
- `LaunchProfile` (`string`): Compose-Profil, das für Stable Diffusion aktiviert sein muss. Standard: `diffusion`.
- `SmokeTestPrompt` (`string`): Kurzer Prompt-Platzhalter für zukünftige Smoke-Tests. Standard: `Erzeuge ein 64x64-Diagnosebild`.

## `ResourceScheduler`
- `GpuUtilisationThreshold` (`double`): Bevorzugte Auslastungsobergrenze, die in der Konfiguration hinterlegt ist. Standard: `0.5` (50 %).
- `GpuMemoryThreshold` (`double`): Bevorzugte VRAM-Obergrenze, die in der Konfiguration hinterlegt ist. Standard: `0.8` (80 %).
- `PollIntervalSeconds` (`int`): Platzhalter für die Abtastfrequenz zukünftiger GPU-Telemetrie. Standard: `5` (Entwicklungs-Override reduziert auf `3`).
- `QueueBackoffSeconds` (`int`): Platzhalter-Verzögerung für den erneuten Versuch wartender Jobs. Standard: `30` (Entwicklungs-Override reduziert auf `10`).

> **Hinweis:** Der Worker zeichnet derzeit Zielwerte der Ressourcenrichtlinien zur operativen Transparenz auf; eine automatische Durchsetzung ist noch nicht implementiert.
