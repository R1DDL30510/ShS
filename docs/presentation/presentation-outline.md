# SecureHomeSystem Präsentations-Outline

Nutze diese Outline, um ein ausgereiftes Folienset (≈15 Slides) für Stakeholder zu erstellen. Jede Folie listet Erzählpunkte, Datenquellen und einen Screenshot-Platzhalter samt Hinweis, was du in deiner Umgebung aufnehmen solltest.

> 💼 **Release-Day-Link:** Kopple diese Outline mit `docs/operations/release-day-playbook.md`, damit Gesprächspunkte, Demo-Schritte und Revision Flags zwischen Foliensatz und Live-Präsentation synchron bleiben.

## Folie 1 – Titel & Vision
- Lösungsname nennen: „SecureHomeSystem – KI-gestützte Home-Orchestration“.
- Tagline: „Koordiniert LLM- und Bild-Workloads für Home-Automation“.
- Presenter: Name, Datum, Organisation.
- **Screenshot-Platzhalter:** `[[Screenshot: Repository-Banner oder Logo-Konzept]]`
  - Sauberes Hero-Grafikmotiv aufnehmen (Repository-Splash oder Compose-Stack-Maskottchen).
  - Falls keine Grafik existiert, temporär einen Farbverlauf einsetzen und später ersetzen.

## Folie 2 – Agenda
- 4–5 Abschnitte auflisten: Überblick, Architektur, Komponenten, Betrieb, Roadmap.
- Demo-Checkpoints (OpenWebUI, Stable Diffusion) erwähnen.
- **Screenshot-Platzhalter:** `[[Screenshot: Inhaltsverzeichnis-Layout]]`
  - Nach Finalisierung die Outline exportieren, um den Flow zu zeigen; bei der Gestaltung durch ein stilisiertes Agenda-Grafik ersetzen.

## Folie 3 – Problem Statement & Ziele
- Orchestrierungs-Herausforderung erläutern (mehrere KI-Dienste, GPU-Contention, Observability).
- Zielgruppe benennen (Home-Lab-Operator:innen, AI-Tüftler:innen).
- Ziele zusammenfassen: Monitoring bündeln, GPU-Richtlinien kommunizieren, Deployments vereinfachen.
- **Screenshot-Platzhalter:** `[[Screenshot: Whiteboard-Foto oder Diagramm der aktuellen Pain Points]]`
  - Annotierten Sketch fotografieren oder ein digitales Whiteboard der aktuellen Herausforderungen erstellen.

## Folie 4 – Lösungsüberblick
- .NET-9-Worker, Docker-Compose-Stack und generative KI-Werkzeuge beschreiben.
- Dienste Rollen zuordnen (Worker = Orchestrator, OpenWebUI = LLM-Frontend, Qdrant = Vektorstore, AUTOMATIC1111 = Bildgenerierung).
- **Screenshot-Platzhalter:** `[[Screenshot: README Overview Abschnitt]]`
  - `README.md` im IDE-Preview öffnen, Overview-Tabelle heranzoomen und sauber capturen.

## Folie 5 – High-Level-Architektur
- Interaktion zwischen Worker, Docker-Daemon, Containern und Host-GPU darstellen.
- Label-basierte Erkennung (`shs.role`) und Environment-Konfiguration erwähnen.
- **Screenshot-Platzhalter:** `[[Screenshot: Architekturdiagramm]]`
  - Diagramm bauen/exportieren: Worker-Container ↔ Docker-Socket ↔ OpenWebUI/Qdrant/AUTOMATIC1111.
  - Tools: draw.io, Visio oder PowerPoint SmartArt.

## Folie 6 – Compose-Stack-Topologie
- Compose-Profile (`worker`, `diffusion`) und optionale Komponenten hervorheben.
- Volume-Mounts und GPU-Reservierungen zusammenfassen.
- Auf `docker/compose.yaml` als maßgebliche Konfiguration verweisen.
- **Screenshot-Platzhalter:** `[[Screenshot: Docker-Desktop-Containeransicht]]`
  - Stack starten, Docker Desktop öffnen, nach Projekt `shs-stack` filtern und laufende Services aufnehmen.

## Folie 7 – SecureHomeSystem Worker Deep Dive
- Worker-Aufgaben skizzieren: Konfiguration laden, Container erkennen, Status alle 30 Sekunden loggen.
- `Worker.cs`-Detection-Loop und `IDockerServiceDetector`-Abstraktion hervorheben.
- Neue `/health`- und `/live`-Endpunkte aus `Program.cs` für Monitoring-Integrationen nennen.
- **Screenshot-Platzhalter:** `[[Screenshot: Worker-Logs im Terminal]]`
  - `docker compose -f docker/compose.yaml logs -f shs-worker` ausführen und einen Abschnitt mit Erkennungen festhalten.

## Folie 8 – OpenWebUI & Qdrant
- Rolle von OpenWebUI (LLM-UX) und Qdrant (RAG) erklären.
- Auf Umgebungsvariablen aus `docker/.env` verweisen (`OLLAMA_BASE_URL`, `WEBUI_PORT`, …).
- Standard-Authentifizierung (`OPENWEBUI_AUTH=false`) und Datenverzeichnisse erwähnen.
- **Screenshot-Platzhalter:** `[[Screenshot: OpenWebUI Landing Page]]`
  - `http://localhost:3003` öffnen und das Dashboard nach Modell-Load aufnehmen.

## Folie 9 – Stable Diffusion (AUTOMATIC1111)
- Optionales Diffusion-Profil und GPU-Reservierungen erläutern.
- Startup-Patch-Skript (`docker/automatic1111_patch.py`) und VRAM-schonende Flags nennen.
- Voraussetzungen hervorheben: NVIDIA-Treiber, `nvidia-smi`.
- **Screenshot-Platzhalter:** `[[Screenshot: AUTOMATIC1111 txt2img Tab]]`
  - Mit aktivem Diffusion-Profil `http://localhost:7860` öffnen und UI nach dem Laden aufnehmen.

## Folie 10 – Konfigurationsmanagement
- Schlüsselsettings aus `SecureHomeSystem/appsettings.json` zusammenfassen (Docker-Erkennung, Service-Endpunkte, Resource Scheduler).
- Für vollständige Schlüssel auf `docs/reference/configuration.md` verweisen.
- Environment-Overrides via `docker/.env` erwähnen.
- **Screenshot-Platzhalter:** `[[Screenshot: appsettings.json in der IDE]]`
  - Relevante Abschnitte (Docker, Services, ResourceScheduler) mit Syntax-Highlighting hervorheben.

## Folie 11 – Betrieb & Runbook
- Verfahren aus `docs/operations/runbook.md` referenzieren (Statuschecks, Neustarts, Troubleshooting).
- GPU-Policy-Koordination (`docs/operations/gpu-policy.md`) erwähnen.
- Zentrale Log-Ablage (`/logs/worker`, `/logs/services`, Cursor unter `/logs/state`) inklusive Rotation (`LogCollector:Rotation`) und Zugriff durch Operator:innen betonen.
- Manuelle Durchsetzung der GPU-Geländer (Revision Flag) hervorheben.
- **Screenshot-Platzhalter:** `[[Screenshot: gerendertes Runbook-Markdown]]`
  - IDE-Markdown-Preview oder Doku-Renderer nutzen, um die Operator-Guidance zu zeigen.

## Folie 12 – Deployment-Workflow
- Schritte für lokales Compose-Bring-up, profilbasierte Starts und Log-Validierung.
- Quickstart-Kommandos aus `docs/setup/docker-compose.md` aufnehmen.
- `.env`-Anpassungen für Host-Pfade und Ports erwähnen.
- **Screenshot-Platzhalter:** `[[Screenshot: Terminal mit docker compose up]]`
  - Erfolgreichen Start zeigen, bei dem Services `healthy`/`running` werden.

## Folie 13 – Demo-Plan & Checkpoints
- Demo-Ablauf skizzieren:
  1. Worker-Profil starten und OpenWebUI/Qdrant verifizieren.
  2. Diffusion-Profil aktivieren und Stable Diffusion validieren.
  3. Worker-Logs zeigen, die Services entdecken.
- Checkpoints mit Kommandos aus `docs/setup/docker-compose.md` verknüpfen.
- **Screenshot-Platzhalter:** `[[Screenshot: Checklist oder Planer]]`
  - Projektmanagement-Tool oder Checkliste zeigen, die du live nutzt.

## Folie 14 – Risiken, Flags & Folgeaktionen
- Offene Punkte vor dem Release präsentieren:
  - Keine automatisierten Tests (`README.md` – Abschnitt **Revision Flags**).
  - GPU-Policy-Automatisierung ausstehend (`README.md` – Abschnitt **Revision Flags**, `docs/operations/gpu-policy.md`).
  - Zentrale Logs liegen als Dateien vor; Query-/Alerting-Layer fehlt weiterhin (`README.md` – Abschnitt **Logging & Observability**).
  - GPU-Dokumentation zu Limits aktuell halten (`docs/setup/docker-compose.md` – Abschnitt **GPU-Limits**).
- Verantwortlichkeiten und Zeitpläne vorschlagen, um jedes Flag zu schließen.
- **Screenshot-Platzhalter:** `[[Screenshot: Issue-Tracker-Board mit Follow-ups]]`
  - Bevorzugtes Tool (GitHub Projects, Jira) verwenden, um Aufgaben je Flag zu zeigen.

## Folie 15 – Roadmap & Nächste Schritte
- Feature-Roadmap: automatisiertes Scheduling, Health-Endpunkte, Test-Harness, Telemetrie-Dashboards.
- Beitragende einladen und Beitragsprozess erläutern (`README.md` Support-Abschnitt).
- Call to Action geben (z. B. „Im Home-Lab pilotieren“, „Testsuites beitragen“).
- **Screenshot-Platzhalter:** `[[Screenshot: Roadmap-Timeline-Grafik]]`
  - Timeline-Diagramm (PowerPoint SmartArt oder anderes Tool) mit kommenden Meilensteinen erzeugen.

## Appendix – Referenzmaterial
- Wichtige Docs verlinken: `README.md`, `docs/reference/configuration.md`, `docs/operations/runbook.md`, `docs/operations/gpu-policy.md`, `docs/setup/docker-compose.md`.
- Environment-Übersicht (`docker/.env`), Compose-Kommando-Cheatsheet und Repository-Struktur aufnehmen.
- **Screenshot-Platzhalter:** `[[Screenshot: Repository-Struktur in der IDE]]`
  - IDE-Explorer mit Top-Level-Verzeichnissen und Schlüsseldateien aufnehmen.

---

### Tipps für die Folioproduktion
- Markdown-Inhalte direkt in den Foliennotizen wiederverwenden, um technische Genauigkeit zu sichern.
- Einheitliches Screenshot-Styling beibehalten (gleiche Auflösung, Dark/Light Mode konsistent).
- Sensible Hostnamen oder IDs vor externer Weitergabe unkenntlich machen.
- Revision Flags sichtbar lassen, bis Maßnahmen umgesetzt sind; Slides sofort aktualisieren, sobald sich der Code ändert.
