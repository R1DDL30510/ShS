# SecureHomeSystem-Stack

## Überblick
SecureHomeSystem ist ein .NET-9-Worker-Dienst, der einen KI-gestützten Home-Automation-Stack überwacht. Der Worker lädt Laufzeiteinstellungen aus `appsettings.json`, registriert Docker- und Serviceendpunkt-Optionen und führt einen gehosteten Background Service für kontinuierliche Orchestrierungsaufgaben aus. Der `Worker`-Dienst protokolliert konfigurierte Endpunkte, stößt wiederholt die Docker-Erkennung an und meldet den Status der verwalteten Container alle 30 Sekunden.

## Stack-Architektur
Das Projekt kombiniert einen schlanken Orchestrierungs-Worker mit einer Docker-Compose-Workload, die generative KI-Werkzeuge und unterstützende Dienste enthält:

| Komponente | Beschreibung |
| --- | --- |
| `shs-worker` | Containerisierter Build des .NET-Workers mit Zugriff auf den Docker-Socket des Hosts für Laufzeiterkennung und Orchestrierung. |
| `open-webui` | OpenWebUI-Front-End für Ollama, konfiguriert über Umgebungsvariablen und mit Labels versehen, damit der Worker es erkennt. |
| `qdrant` | Vektordatenbank, die von OpenWebUI für Retrieval-Augmented-Workflows verwendet wird. |
| `automatic1111` | Stable-Diffusion-Web-UI-Container mit GPU-Reservierungen, Modell-Mounts und Startpatching für Kompatibilitätsanpassungen. |

Compose-Labels (`shs.role`) ermöglichen es dem Worker, beim Parsen der `docker ps`-Ausgabe laufende Container logischen Diensten zuzuordnen.

## Zentrale Funktionen
- **Docker-bewusste Orchestrierung** – Erkennt gelabelte Container über die Docker-CLI, stellt Status-Telemetrie bereit und geht sanft mit CLI-Fehlern oder Abbrüchen um.
- **Konfigurierbare Serviceendpunkte** – Zentralisierte Optionsobjekte definieren Basis-URLs, GPU-Schwellenwerte und Health Checks für Ollama-, OpenWebUI- und Stable-Diffusion-Dienste.
- **HTTP-Health-Endpunkte** – Die Routen `/health` und `/live` exponieren den Worker-Status für Compose-Health-Checks und externe Überwachung.
- **Zentrale Log-Erfassung** – Serilog schreibt Worker-Ereignisse als JSON unter `/logs/worker`, während ein Hintergrundsammler Container-Stdout pro Dienst in `/logs/services` persistiert und Cursor-Dateien unter `/logs/state` pflegt.
- **GPU-Richtlinienkonfiguration** – Ressourcenscheduler-Schwellen werden in der Konfiguration erfasst, um zukünftige Automatisierung zu ermöglichen, während Betriebsteams die Limits weiterhin manuell durchsetzen.
- **Betriebliche Anleitung** – Runbooks dokumentieren Protokollpfade, Neustartverfahren, Health Checks und Eskalationspfade für den Produktivbetrieb.

## Voraussetzungen
- [.NET SDK 9.0](https://dotnet.microsoft.com/) für lokale Builds des Worker-Dienstes.
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) mit Compose V2 und aktiviertem GPU-Passthrough (NVIDIA).
- NVIDIA-Treiber sowie `nvidia-smi`-Verfügbarkeit für GPU-Telemetrie, wenn Stable Diffusion oder Ollama GPU-Beschleunigung nutzen.

## Erste Schritte
1. Repository klonen und eine Datei `docker/.env` erstellen (unterstützte Overrides siehe `docker/compose.yaml`), damit host-spezifische Pfade und GPU-Einstellungen bereitgestellt werden können.
2. Stack bauen und starten:
   ```bash
   docker compose -f docker/compose.yaml up -d --build
   ```
3. Logs für die erste Erkennung verfolgen:
   ```bash
   docker compose -f docker/compose.yaml logs -f shs-worker
   ```
4. OpenWebUI unter `http://localhost:3003` aufrufen (sofern nicht überschrieben) und die Einsatzbereitschaft von Stable Diffusion unter `http://localhost:7860` prüfen.

Der Worker meldet jeden erkannten Dienst mit Container-ID, Image, Status und Running-Flag, was eine schnelle Validierung der Stack-Gesundheit ermöglicht.

## Konfiguration
Laufzeiteinstellungen befinden sich in `SecureHomeSystem/appsettings.json` und können beim Betrieb im Container über Umgebungsvariablen überschrieben werden. Wichtige Abschnitte sind:
- `Docker`: Compose-Dateipfad, Projektname, Auto-Start-Profile und Auswahlkriterien für die Erkennung.
- `Services`: Endpunkt-Metadaten für Ollama, OpenWebUI und Stable Diffusion, einschließlich Health-Check-Pfaden und GPU-Speicherlimits.
- `ResourceScheduler`: GPU-Auslastungsschwellen und Polling-Frequenz für zukünftige Scheduling-Funktionen.
- `Health`: Port- und Pfadkonfiguration für die Worker-Endpunkte `/health` und `/live`.

Siehe `docs/reference/configuration.md` für die vollständige Parameterreferenz und Standardwerte, einschließlich der verschachtelten Erkennungseinstellungen unter `Docker:Detection` und der Service-Endpunkt-Overrides.

## Logging & Observability
- Worker-Telemetrie wird als rotierende JSON-Dateien unter `/logs/worker/worker-<Datum>.json` abgelegt. Dateien rotieren täglich oder ab 10 MB und behalten standardmäßig die letzten 14 Segmente.
- Container-Stdout sammelt der `ContainerLogCollector` in `/logs/services/<service>.log`; Cursor unter `/logs/state/<service>.cursor` verhindern Duplikate nach Neustarts. Logs rotieren bei 10 MB oder nach 7 Tagen und der Worker behält bis zu fünf Archive pro Dienst.
- Überschreibe das Host-Mount über die Umgebungsvariable `LOG_DIR` (Standard `../data/logs`) in `docker/.env`; die Compose-Datei bindet dieses Verzeichnis als `/logs` in den Worker-Container ein.
- Für schnelle Analysen eignet sich weiterhin `docker compose -f docker/compose.yaml logs -f shs-worker`, für Audit Trails, Vergleiche über Neustarts hinweg und zukünftige automatisierte Tests sind jedoch die persistierten Dateien maßgeblich.

## Betrieb
- **Statusprüfungen:** `docker compose -f docker/compose.yaml ls` und `docker ps --filter label=shs.role`, um die Container-Gesundheit zu bestätigen.
- **Worker-Health:** `curl http://localhost:5080/health` (anpassbar über `WORKER_HEALTH_PORT`) liefert HTTP 200.
- **Logs:** `/logs/worker/worker-*.json` für strukturierte Worker-Ereignisse oder `/logs/services/<service>.log` für Container-Stdout-Snapshots; `docker compose -f docker/compose.yaml logs` für Live-Streaming bei Bedarf.
- **Neustarts:** Einzelnen Dienst mit `docker compose restart <service>` gezielt neu starten oder den gesamten Stack mit `down`/`up -d` recyceln.
- **GPU-Verwaltung:** Folge der GPU-Policy, um VRAM-Limits und Queue-Verhalten bei Engpässen einzuhalten.
- **Release-Bereitschaft:** Konsultiere `docs/operations/release-day-playbook.md` für die Checkliste am Tag der Veröffentlichung, das Demo-Skript und die Revision Flags für Stakeholder.

## Entwicklungs-Workflow
- Führe den Worker lokal mit `dotnet run --project SecureHomeSystem` aus, um ohne Container zu iterieren.
- Führe die automatisierten Tests über `pwsh ./scripts/build.ps1` aus (restore → format → build → test) oder gezielt per `dotnet test ShS.slnx --filter "Category=Unit"`.
- Container-Builds installieren die Docker-CLI im Runtime-Image, sodass der Worker beim Einsatz in Compose mit dem Host-Daemon kommunizieren kann.

## Testing & Continuous Integration
- Tests liegen unter `tests/SecureHomeSystem.Tests` auf Basis von xUnit 3 + FluentAssertions mit den Traits `Unit`, `Integration` und künftig `Smoke` (siehe `docs/TESTING.md`).
- `scripts/build.ps1` erzwingt restore → format → build → test; beim Iterieren kann `-SkipFormat` genutzt werden, um Format-Prüfungen zu überspringen.
- `scripts/smoke.ps1` stellt den Compose-Stack bereit und führt Docker-basierte Smoke-Tests aus, sofern eine Docker-Umgebung vorhanden ist.
- Der GitHub-Actions-Workflow `.github/workflows/ci.yml` führt Build und Tests unter Windows und Linux aus; über „Run workflow“ und `run_smoke=true` lassen sich Compose-Smoke-Checks einschließen.

## Repository-Struktur
```
SecureHomeSystem/      # Quellcode, Optionsklassen, Infrastruktur und gehosteter Worker
  Configuration/      # Stark typisierte Optionen für Docker, Dienste, Logging und Scheduling
  Services/           # Abstraktion der Docker-Erkennung und Container-Log-Sammler
  Models/             # Datenverträge für erkannte Dienste
  Dockerfile          # Multi-Stage-Build für das Worker-Container-Image
  appsettings.json    # Standard-Laufzeitkonfiguration

docker/               # Compose-Stack-Definition und Hilfsskripte
  compose.yaml
  automatic1111_patch.py

docs/                 # Betriebs- und Konfigurationsreferenzen
  reference/
  operations/
  setup/

tests/                # xUnit-3-Testprojekt mit Unit- und Integration-Abdeckung
scripts/              # build.ps1 (Lint/Build/Test) & smoke.ps1 (Docker-Smoke-Tests)
.github/workflows/    # GitHub-Actions-CI-Pipeline
```

## Support und Beitrag
Betriebliche Themen und Funktionsvorschläge sollten über den Issue-Tracker des Repositories verfolgt werden. Beim Beitragen von Code:
1. Forke das Repository und erstelle einen Feature-Branch.
2. Halte dich an die .NET-Coding-Conventions und stelle sicher, dass neue Dienste Docker-Labels zur Erkennung enthalten.
3. Liefere Dokumentations-Updates (Runbook oder Konfigurationsreferenz) zusammen mit Funktionsänderungen.
4. Öffne einen Pull Request mit Kontext, Testnachweisen und Überlegungen zum Rollback.

## Revision Flags
- ✅ **Automatisiertes Testing** – Grundlegende Unit-/Integration-Abdeckung und CI sind vorhanden. Baue Docker-basierte Smoke-Pipelines aus, sobald Runner Compose-Workloads unterstützen.
- ⚠️ **GPU-Policy-Automatisierung** – Ressourcengrenzen sind derzeit nur empfehlend. Aktualisiere die GPU-Richtliniendokumente, sobald Durchsetzungslogik bereitsteht.
- ?? **Log-Analytics** – Logs persistieren jetzt unter `/logs`, aber noch wertet keine Indexing- oder Alerting-Schicht sie aus. Plane eine Loki/Elastic/Promtail-Integration, sobald priorisiert.
