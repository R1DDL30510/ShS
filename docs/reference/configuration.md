# Konfigurationsreferenz

Der Worker lädt die Konfiguration aus `SecureHomeSystem/appsettings.json`, wobei Überschreibungen über Umgebungsvariablen oder zusätzliche JSON-Dateien wie `appsettings.Development.json` angewendet werden. Die folgenden Abschnitte listen die unterstützten Schlüssel und ihre Standardwerte in diesem Repository auf.

## `Docker`
- `ComposeFilePath` (`string`): Pfad zur Compose-Datei. Standard: `docker/compose.yaml`.
- `EnvironmentFile` (`string`): Relativer Pfad zur Compose-`.env`-Datei. Standard: `docker/.env`.
- `ProjectName` (`string`): Compose-Projektname, der bei CLI-Befehlen verwendet wird. Standard: `shs-stack`.
- `AutoStartProfiles` (`string[]`): Compose-Profile, die der Worker zu starten versucht (Repository-Standard: `["worker"]`, Entwicklungs-Override fügt `"diffusion"` hinzu).
- `Detection` (`ServiceDetectionOptions`): Verschachtelte Einstellungen, die steuern, wie Docker-Container zugeordnet werden.

### `Docker:Detection`
- `LabelSelector` (`Dictionary<string,string>`): Erwartete Docker-Label-Filter, nach logischem Dienstnamen indiziert. Standardwerte ordnen den Worker den in `docker/compose.yaml` gesetzten `shs.role`-Labels zu (`open-webui`, `qdrant`, `stable-diffusion`).
- `StartupTimeoutSeconds` (`int`): Zeitfenster (Sekunden), in dem ein Dienst erscheinen muss, bevor die Erkennung einen Timeout meldet. Standard: `120`.
- `RetryCount` (`int`): Anzahl der Erkennungsversuche, bevor eskaliert wird. Standard: `3`.

## `Services`
### `Services:Ollama`
- `BaseUrl` (`string`): Endpunkt, den der Worker in Logs verwendet. Repository-Standard: `http://host.docker.internal:11434`.
- `MaxGpuMemoryFraction` (`double`): Gewünschtes VRAM-Limit, das Operator:innen und Downstream-Tools kommuniziert wird. Standard: `0.8` (80 %).
- `HealthEndpoint` (`string`): Pfad für Health-Checks. Standard: `/api/tags`.

### `Services:OpenWebUi`
- `BaseUrl` (`string`): URL, die der Worker ankündigt. Repository-Standard: `http://localhost:3003` (entspricht dem Compose-Port-Binding).
- `RequireAuth` (`bool`): Spiegelt das Authentifizierungsflag wider, das dem OpenWebUI-Container übergeben wird. Standard: `false`.
- `HealthEndpoint` (`string`): Pfad für manuelle Health-Checks. Standard: `/api/system/info`.

### `Services:StableDiffusion`
- `BaseUrl` (`string`): Endpunkt der Stable-Diffusion-Oberfläche. Repository-Standard: `http://localhost:7860`.
- `LaunchProfile` (`string`): Compose-Profil, das für Stable Diffusion aktiviert sein muss. Standard: `diffusion`.
- `SmokeTestPrompt` (`string`): Kurzer Prompt-Platzhalter für zukünftige Smoke-Tests. Standard: `Generate a 64x64 diagnostic image`.

## `ResourceScheduler`
- `GpuUtilisationThreshold` (`double`): Bevorzugte Auslastungsobergrenze, die in der Konfiguration hinterlegt ist. Standard: `0.5` (50 %).
- `GpuMemoryThreshold` (`double`): Bevorzugte VRAM-Obergrenze, die in der Konfiguration hinterlegt ist. Standard: `0.8` (80 %).
- `PollIntervalSeconds` (`int`): Platzhalter für die Abtastfrequenz zukünftiger GPU-Telemetrie. Standard: `5` (Entwicklungs-Override reduziert auf `3`).
- `QueueBackoffSeconds` (`int`): Platzhalter-Verzögerung für den erneuten Versuch wartender Jobs. Standard: `30` (Entwicklungs-Override reduziert auf `10`).

> **Hinweis:** Der Worker zeichnet derzeit Zielwerte der Ressourcenrichtlinien zur operativen Transparenz auf; eine automatische Durchsetzung ist noch nicht implementiert.

## `Health`
- `Port` (`int`): TCP-Port, den der Worker für Health- und Liveness-Endpunkte bindet. Standard: `5080`.
- `HealthPath` (`string`): Pfad, der über ASP.NET-Core-Health-Checks exponiert wird und von der Compose-Health-Probe genutzt wird. Standard: `/health`.
- `LivenessPath` (`string`): Minimaler JSON-Endpunkt für schnelle manuelle Checks. Standard: `/live`.

## `Serilog`
- `Using` (`string[]`): Assemblies, die konfigurierte Sinks bereitstellen. Standard umfasst `Serilog.Sinks.Console` und `Serilog.Sinks.File`.
- `MinimumLevel` (`object`): Baseline-Log-Level plus Überschreibungen für Namespaces. Standard protokolliert Microsoft-Kategorien auf `Warning`, während die App auf `Information` bleibt.
- `WriteTo[0]` (`Console`): Sendet Logs an stdout, damit Docker-Logs verfügbar sind.
- `WriteTo[1]` (`File`): Persistiert JSON-formatierte Worker-Logs unter `/logs/worker/worker-.json`. Rotation behält 14 Dateien, rotiert täglich oder ab 10 MB.
- `Properties:Application` (`string`): Statisches Property, das jedem Log-Event hinzugefügt wird. Standard: `SecureHomeSystem.Worker`.

## `LogStorage`
- `RootPath` (`string`): Basisverzeichnis für alle persistierten Logs und Cursor. Standard: `/logs` (aus `${LOG_DIR}` in Compose gemountet).
- `WorkerFolderName` (`string`): Unterordner für Serilog-Ausgaben. Standard: `worker`.
- `ServicesFolderName` (`string`): Unterordner für geharvestete Container-Logs. Standard: `services`.
- `StateFolderName` (`string`): Unterordner für dienstspezifische Cursor-Dateien. Standard: `state`.

## `LogCollector`
- `Enabled` (`bool`): Schalter für den Docker-Log-Harvester. Standard: `true`.
- `PollIntervalSeconds` (`int`): Verzögerung zwischen Sammelvorgängen. Standard: `30`.
- `InitialLookbackMinutes` (`int`): Zeitraum, der beim ersten Lauf ohne Cursor zurückgespult wird. Standard: `10`.
- `Rotation` (`LogRotationOptions`): Aufbewahrungsregeln für `/logs/services/<service>.log`.
  - `MaxFileSizeBytes` (`long`): Rotiert, sobald die aktive Log-Datei diese Größe erreicht. Standard: `10485760` (10 MB).
  - `MaxFileAgeDays` (`int`): Rotiert, wenn die aktive Log-Datei mindestens so alt ist. Standard: `7`.
  - `MaxArchiveFiles` (`int`): Maximale Anzahl aufzubewahrender Archivdateien pro Dienst (neueste zuerst). Standard: `5`.
