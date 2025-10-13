# SecureHomeSystem-Präsentationsleitfaden

Verwenden Sie diesen Leitfaden, um ein ausgearbeitetes Folienset (≈15 Folien) für Stakeholder zu erstellen. Jede Folie listet Erzählpunkte, Datenquellen und einen Screenshot-Platzhalter mit Hinweisen darauf, was in Ihrer Umgebung festzuhalten ist.

> 💼 **Link für den Releasetag:** Kombinieren Sie diesen Leitfaden mit `docs/operations/release-day-playbook.md`, damit Gesprächspunkte, Demo-Schritte und Revisionshinweise zwischen Foliensatz und Live-Präsentation synchron bleiben.

## Folie 1 – Titel & Vision
- Stellen Sie den Lösungsnamen vor: „SecureHomeSystem – KI-gestützte Hausorchestrierung“.
- Slogan: „Koordination von LLM- und Bild-Workloads für die Heimautomation“.
- Fügen Sie Name der präsentierenden Person, Datum und Organisation hinzu.
- **Screenshot-Platzhalter:** `[[Screenshot: Repository-Banner oder Logokonzept]]`
  - Halten Sie eine ansprechende Hero-Grafik fest (Repository-Splash oder Compose-Stack-Maskottchen).
  - Falls keine Grafik existiert, platzieren Sie einen temporären Farbverlauf und planen Sie später einen Austausch ein.

## Folie 2 – Agenda
- Listen Sie 4–5 Abschnitte auf: Überblick, Architektur, Komponenten, Betrieb, Roadmap.
- Nennen Sie Demo-Kontrollpunkte (OpenWebUI, Stable Diffusion).
- **Screenshot-Platzhalter:** `[[Screenshot: Inhaltsverzeichnis-Layout]]`
  - Exportieren Sie den Folienskeleton nach der Finalisierung, um den Ablauf zu zeigen; ersetzen Sie ihn beim Design durch eine stilisierte Agenda-Grafik.

## Folie 3 – Problemstellung & Ziele
- Erläutern Sie die Orchestrierungsherausforderung (mehrere KI-Dienste, GPU-Konkurrenz, Beobachtbarkeit).
- Identifizieren Sie die Zielgruppe (Home-Lab-Betreibende, KI-Tüftler:innen).
- Fassen Sie die Ziele zusammen: Monitoring vereinheitlichen, GPU-Richtlinien verständlich kommunizieren, Deployments vereinfachen.
- **Screenshot-Platzhalter:** `[[Screenshot: Whiteboard-Foto oder Diagramm aktueller Schmerzpunkte]]`
  - Fotografieren Sie eine annotierte Skizze oder erstellen Sie ein digitales Whiteboard mit den aktuellen Herausforderungen.

## Folie 4 – Lösungsüberblick
- Beschreiben Sie den .NET-9-Worker, den Docker-Compose-Stack und die generativen KI-Werkzeuge.
- Ordnen Sie Dienste ihren Rollen zu (Worker = Orchestrator, OpenWebUI = LLM-Frontend, Qdrant = Vektorspeicher, AUTOMATIC1111 = Bildgenerierung).
- **Screenshot-Platzhalter:** `[[Screenshot: README-Überblicksabschnitt]]`
  - Öffnen Sie `README.md` in der IDE-Vorschau, zoomen Sie in den Überblickstext und nehmen Sie einen sauberen Ausschnitt auf.

## Folie 5 – Architektur auf hoher Ebene
- Stellen Sie die Interaktion zwischen Worker, Docker-Daemon, Containern und Host-GPU dar.
- Erwähnen Sie labelbasierte Erkennung (`shs.role`) und Umgebungsvariablen.
- **Screenshot-Platzhalter:** `[[Screenshot: Architekturdiagramm]]`
  - Erstellen oder exportieren Sie ein Diagramm mit: Worker-Container ↔ Docker-Socket ↔ OpenWebUI/Qdrant/AUTOMATIC1111.
  - Werkzeuge: draw.io, Visio oder PowerPoint SmartArt.

## Folie 6 – Compose-Stack-Topologie
- Heben Sie Compose-Profile (`worker`, `diffusion`) und optionale Komponenten hervor.
- Fassen Sie Volume-Mounts und GPU-Reservierungseinstellungen zusammen.
- Verweisen Sie auf `docker/compose.yaml` als maßgebliche Konfiguration.
- **Screenshot-Platzhalter:** `[[Screenshot: Docker-Desktop-Containeransicht]]`
  - Starten Sie den Stack, öffnen Sie Docker Desktop, filtern Sie nach Projektname `shs-stack` und halten Sie die laufenden Dienste fest.

## Folie 7 – SecureHomeSystem-Worker im Detail
- Skizzieren Sie die Aufgaben des Workers: Konfiguration laden, Container erkennen, Status alle 30 Sekunden protokollieren.
- Weisen Sie auf die Erkennungsschleife in `Worker.cs` und die Abstraktion `IDockerServiceDetector` hin.
- Erwähnen Sie das fehlende HTTP-Health-Check-Endpunkt (Revisionshinweis).
- **Screenshot-Platzhalter:** `[[Screenshot: Worker-Logs im Terminal]]`
  - Führen Sie `docker compose -f docker/compose.yaml logs -f shs-worker` aus und zeichnen Sie einen Abschnitt mit Erkennungen auf.

## Folie 8 – OpenWebUI & Qdrant
- Beschreiben Sie, wie OpenWebUI die LLM-Nutzeroberfläche bereitstellt und Qdrant für RAG nutzt.
- Verweisen Sie auf Umgebungsvariablen aus `docker/.env` (`OLLAMA_BASE_URL`, `WEBUI_PORT` usw.).
- Notieren Sie die Standardauthentifizierung (`OPENWEBUI_AUTH=false`) und Datenverzeichnisse.
- **Screenshot-Platzhalter:** `[[Screenshot: OpenWebUI-Startseite]]`
  - Rufen Sie `http://localhost:3003` auf und erfassen Sie das Haupt-Dashboard, sobald Modelle geladen sind.

## Folie 9 – Stable Diffusion (AUTOMATIC1111)
- Erklären Sie das optionale Diffusionsprofil und die GPU-Reservierungen.
- Erwähnen Sie das Startpatch-Skript (`docker/automatic1111_patch.py`) und VRAM-schonende Flags.
- Heben Sie Voraussetzungen hervor: NVIDIA-Treiber, `nvidia-smi`.
- **Screenshot-Platzhalter:** `[[Screenshot: AUTOMATIC1111-Tab „txt2img“]]`
  - Mit aktivem Diffusionsprofil `http://localhost:7860` öffnen und die Oberfläche nach dem Laden aufnehmen.

## Folie 10 – Konfigurationsmanagement
- Fassen Sie die wichtigsten Einstellungen aus `SecureHomeSystem/appsettings.json` zusammen (Docker-Erkennung, Serviceendpunkte, Ressourcenplaner).
- Verweisen Sie auf `docs/reference/configuration.md` für die vollständige Schlüsselliste.
- Heben Sie Umgebungsüberschreibungen via `docker/.env` hervor.
- **Screenshot-Platzhalter:** `[[Screenshot: appsettings.json in der IDE]]`
  - Markieren Sie relevante Abschnitte (Docker, Services, ResourceScheduler) mit IDE-Syntaxhervorhebung.

## Folie 11 – Betrieb & Runbook
- Verweisen Sie auf Verfahren in `docs/operations/runbook.md` (Statusprüfungen, Neustarts, Fehlersuche).
- Nennen Sie die GPU-Richtlinienabstimmung (`docs/operations/gpu-policy.md`).
- Betonen Sie die manuelle Durchsetzung der GPU-Schutzmaßnahmen (Revisionshinweis).
- **Screenshot-Platzhalter:** `[[Screenshot: gerendertes Runbook-Markdown]]`
  - Nutzen Sie die IDE-Markdown-Vorschau oder eine Dokumentationsseite, um die Betreiberanleitung zu zeigen.

## Folie 12 – Deployment-Workflow
- Schritte für lokales Compose-Starten, profilspezifischen Start und Log-Validierung.
- Integrieren Sie die Quickstart-Befehle aus `docs/setup/docker-compose.md`.
- Weisen Sie auf `.env`-Anpassungen für Hostpfade und Ports hin.
- **Screenshot-Platzhalter:** `[[Screenshot: Terminal mit „docker compose up“]]`
  - Zeigen Sie erfolgreiche Startausgaben mit Diensten im Zustand `healthy` oder `running`.

## Folie 13 – Demoplan & Kontrollpunkte
- Skizzieren Sie den Demoablauf:
  1. Worker-Profil starten und OpenWebUI/Qdrant bestätigen.
  2. Diffusionsprofil aktivieren und Stable Diffusion validieren.
  3. Worker-Logs zeigen, die Dienste erkennen.
- Verknüpfen Sie Kontrollpunkte mit den Befehlen aus `docs/setup/docker-compose.md`.
- **Screenshot-Platzhalter:** `[[Screenshot: Checklisten-Spreadsheet oder Planer]]`
  - Halten Sie das Projektmanagement-Tool oder die Checkliste fest, die Sie während der Live-Demo verwenden.

## Folie 14 – Risiken, Flags & Folgeaktionen
- Stellen Sie offene Punkte dar, die vor der Veröffentlichung geprüft werden müssen:
  - HTTP-Health-Check des Workers fehlt (`README.md:89`, `docs/operations/runbook.md:19`).
  - Keine automatisierten Tests (`README.md:90`).
  - GPU-Richtlinienautomatisierung ausstehend (`README.md:91`, `docs/operations/gpu-policy.md:24`).
  - Weiterleitung an Windows Event Log nicht dokumentiert (`docs/operations/runbook.md:9`).
  - Aktualisierung der GPU-Dokumentation zur Durchsetzung erforderlich (`docs/setup/docker-compose.md:27`).
- Schlagen Sie Verantwortlichkeiten und Zeitpläne zur Behebung jedes Flags vor.
- **Screenshot-Platzhalter:** `[[Screenshot: Issue-Tracker-Board mit Folgeaufgaben]]`
  - Nutzen Sie Ihr bevorzugtes Tracker-Tool (GitHub Projects, Jira), um Aufgaben je Flag anzuzeigen.

## Folie 15 – Roadmap & nächste Schritte
- Feature-Roadmap: Automatisiertes Scheduling, Health-Endpoints, Test-Harness, Telemetrie-Dashboards.
- Laden Sie Mitwirkende ein und umreißen Sie den Beitragspfad (Abschnitt „Support“ in `README.md`).
- Sprechen Sie eine Handlungsaufforderung aus (z. B. „Im Heimlabor pilotieren“, „Test-Suites beitragen“).
- **Screenshot-Platzhalter:** `[[Screenshot: Roadmap-Zeitachsengrafik]]`
  - Erstellen Sie ein Zeitachsendiagramm (PowerPoint SmartArt oder externes Tool) mit den kommenden Meilensteinen.

## Anhang – Referenzmaterial
- Verlinken Sie zentrale Dokumente: `README.md`, `docs/reference/configuration.md`, `docs/operations/runbook.md`, `docs/operations/gpu-policy.md`, `docs/setup/docker-compose.md`.
- Fügen Sie Umweltübersicht (`docker/.env`), Compose-Befehlsübersicht und Repository-Struktur hinzu.
- **Screenshot-Platzhalter:** `[[Screenshot: Repository-Baum in der IDE]]`
  - Erfassen Sie den IDE-Explorer mit den Top-Level-Verzeichnissen und wichtigen Dateien.

---

### Tipps für die Erstellung der Präsentation
- Übernehmen Sie den Markdown-Inhalt direkt in die Foliensprechernotizen, um technische Genauigkeit zu bewahren.
- Halten Sie einen konsistenten Stil über alle Screenshots hinweg (gleiche Auflösung, Dark-/Light-Mode).
- Schwärzen Sie sensible Hostnamen oder IDs, bevor Sie extern teilen.
- Lassen Sie Revisionshinweise sichtbar, bis Maßnahmen umgesetzt sind; aktualisieren Sie die Folien unmittelbar nach Änderungen im Codebestand.
