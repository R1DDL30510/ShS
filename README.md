# SecureHomeSystem-Stack

## Überblick
SecureHomeSystem ist ein .NET-9-Worker-Dienst, der einen KI-gestützten Home-Automation-Stack überwacht. Der Worker lädt Laufzeiteinstellungen aus `appsettings.json`, registriert Docker- und Serviceendpunkt-Optionen und führt einen gehosteten Background Service für kontinuierliche Orchestrierungsaufgaben aus. Der `Worker`-Dienst protokolliert konfigurierte Endpunkte, stößt wiederholt die Docker-Erkennung an und meldet alle 30 Sekunden den Status der verwalteten Container.

## Stack-Architektur
Das Projekt kombiniert einen schlanken Orchestrierungs-Worker mit einer Docker-Compose-Workload, die generative KI-Werkzeuge und unterstützende Dienste enthält:

| Komponente | Beschreibung |
| --- | --- |
| `shs-worker` | Containerisierter Build des .NET-Workers mit Zugriff auf den Docker-Socket des Hosts für Laufzeiterkennung und Orchestrierung. |
| `open-webui` | HomeChatGPT-Front-End (OpenWebUI-Basis) für Ollama, konfiguriert über Umgebungsvariablen und mit Labels versehen, damit der Worker es erkennt. |
| `qdrant` | Vektordatenbank, die vom HomeChatGPT-Frontend für Retrieval-Augmented Workflows verwendet wird. |
| `automatic1111` | Stable-Diffusion-Web-UI-Container mit GPU-Reservierungen, Modell-Mounts und Startpatching für Kompatibilitätsanpassungen. |

Compose-Labels (`shs.role`) ermöglichen es dem Worker, beim Parsen der `docker ps`-Ausgabe laufende Container logischen Diensten zuzuordnen.

## Zentrale Funktionen
- **Docker-bewusste Orchestrierung** – Erkennt gelabelte Container über die Docker-CLI, stellt Status-Telemetrie bereit und geht sanft mit CLI-Fehlern oder Abbrüchen um.
- **Konfigurierbare Serviceendpunkte** – Zentralisierte Optionsobjekte definieren Basis-URLs, GPU-Schwellenwerte und Health Checks für Ollama-, HomeChatGPT- (OpenWebUI) und Stable-Diffusion-Dienste.
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
4. HomeChatGPT (OpenWebUI) unter `http://localhost:3003` aufrufen (sofern nicht überschrieben) und die Einsatzbereitschaft von Stable Diffusion unter `http://localhost:7860` prüfen.

Der Worker meldet jeden erkannten Dienst mit Container-ID, Image, Status und Running-Flag, was eine schnelle Validierung der Stack-Gesundheit ermöglicht.

## Konfiguration
Laufzeiteinstellungen befinden sich in `SecureHomeSystem/appsettings.json` und können beim Betrieb im Container über Umgebungsvariablen überschrieben werden. Wichtige Abschnitte sind:
- `Docker`: Compose-Dateipfad, Projektname, Auto-Start-Profile und Auswahlkriterien für die Erkennung.
- `Services`: Endpunkt-Metadaten für Ollama, HomeChatGPT (OpenWebUI) und Stable Diffusion, einschließlich Health-Check-Pfaden und GPU-Speicherlimits.
- `ResourceScheduler`: GPU-Auslastungsschwellen und Polling-Frequenz für zukünftige Scheduling-Funktionen.

Siehe `docs/reference/configuration.md` für die vollständige Parameterreferenz und Standardwerte, einschließlich der verschachtelten Erkennungseinstellungen unter `Docker:Detection` und Service-Endpunkt-Overrides.

## Betrieb
- **Statusprüfungen:** `docker compose -f docker/compose.yaml ls` und `docker ps --filter label=shs.role`, um die Container-Gesundheit zu bestätigen.
- **Logs:** Verwenden Sie `docker compose logs shs-worker` oder dienstspezifische `docker logs`-Befehle zur Fehlersuche.
- **Neustarts:** Einzelnen Dienst mit `docker compose restart <service>` gezielt neu starten oder den gesamten Stack mit `down`/`up -d` recyceln.
- **GPU-Verwaltung:** Folgen Sie der GPU-Richtlinie, um VRAM-Limits und Queue-Verhalten bei Engpässen einzuhalten.
- **Release-Bereitschaft:** Konsultieren Sie `docs/operations/release-day-playbook.md` für die Checkliste am Tag der Veröffentlichung, das Demo-Skript und relevante Revisionshinweise für Stakeholder.

## Entwicklungs-Workflow
- Führen Sie den Worker lokal mit `dotnet run --project SecureHomeSystem` aus, um ohne Container zu iterieren.
- Führen Sie Unit- oder Integrationstests aus, sobald sie eingeführt werden; **aktuell wird kein automatisiertes Test-Suite mitgeliefert.**
- Container-Builds installieren die Docker-CLI im Runtime-Image, sodass der Worker beim Einsatz in Compose mit dem Host-Daemon kommunizieren kann.

## Repository-Struktur
```
SecureHomeSystem/      # Quellcode, Optionsklassen und gehosteter Worker des .NET-Workers
  Configuration/      # Stark typisierte Optionen für Docker, Dienste und Scheduling
  Services/           # Abstraktion und Implementierung der Docker-Erkennung
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
```

## Support und Beitrag
Betriebliche Themen und Funktionsvorschläge sollten über den Issue-Tracker des Repositories verfolgt werden. Beim Beitragen von Code:
1. Forken Sie das Repository und erstellen Sie einen Feature-Branch.
2. Halten Sie sich an die .NET-Coding-Conventions und stellen Sie sicher, dass neue Dienste Docker-Labels zur Erkennung enthalten.
3. Liefern Sie Dokumentations-Updates (Runbook oder Konfigurationsreferenz) zusammen mit Funktionsänderungen.
4. Öffnen Sie einen Pull Request mit Kontext, Testnachweisen und Überlegungen zum Rollback.

## Revisionshinweise
- ⚠️ **Worker-Health-Endpunkt** – Der Worker besitzt derzeit keinen HTTP-Health-Check; aktualisieren Sie die Dokumentation, sobald ein Endpunkt implementiert ist.
- ⚠️ **Automatisiertes Testing** – Es existieren keine Unit- oder Integrationstests. Fügen Sie Abdeckung hinzu oder aktualisieren Sie die Workflow-Anleitung, sobald Tests verfügbar sind.
- ⚠️ **GPU-Richtlinienautomatisierung** – Ressourcengrenzen sind derzeit nur empfehlend. Aktualisieren Sie die GPU-Richtliniendokumente, sobald Durchsetzungslogik bereitsteht.
