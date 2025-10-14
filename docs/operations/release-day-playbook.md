# Release-Day Playbook

> **Zielgruppe:** Du selbst in ein paar Monaten – oder Kolleg:innen, die kurzfristig einspringen müssen. Dieses Playbook fasst alle relevanten Schritte zusammen, damit der SecureHomeSystem-Release-Tag reibungslos abläuft und jede Entscheidung nachvollziehbar bleibt.

## 1. Vorbereitungs-Timeline (T-7 bis T-1)

| Zeitpunkt | Aufgabe | Referenzen |
| --- | --- | --- |
| T-7 | Projektüberblick auffrischen, Präsentationsoutline mit Stakeholdern abgleichen. | `docs/presentation/presentation-outline.md` (Slides 1–5) |
| T-5 | Compose-Stack lokal durchstarten, offene Labels prüfen. | `docker/compose.yaml`, `SecureHomeSystem/Configuration/DockerOptions.cs` |
| T-4 | GPU-Policy verifizieren, ggf. Limits aktualisieren. | `docs/operations/gpu-policy.md`, `SecureHomeSystem/Configuration/ResourceSchedulerOptions.cs` |
| T-3 | Demo-Skript entwerfen und Worker-Logs aus `/logs/worker` mitschneiden. | `SecureHomeSystem/Worker.cs`, `docs/operations/runbook.md` |
| T-2 | Release-Flags evaluieren und Status in Präsentation aufnehmen. | `README.md` → Abschnitt **Revision Flags** |
| T-1 | End-to-End-Probelauf inkl. Screenshot-Erstellung. | `docs/presentation/presentation-outline.md` → Screenshot-Hinweise |

> 🔁 **Wiederholbarkeit:** Jede Aufgabe notiert, welche Dateien du im Blick behalten musst. Wenn Konfigurationswerte verändert werden, dokumentiere Datum & Grund direkt in diesem Playbook unter „Appendix A“.

## 2. Release-Day Checkliste

1. **Repository-Stand verifizieren**
   - `git status` – Branch & Cleanliness sicherstellen.
   - `git log -5 --oneline` – letzte Commits im Blick behalten.
2. **Environment vorbereiten**
   - `cp docker/.env.example docker/.env` (falls neu aufgesetztes System).
   - Prüfe GPU-Verfügbarkeit: `nvidia-smi`.
3. **Stack starten**
   - `docker compose -f docker/compose.yaml up -d --build`.
   - Optionales Diffusion-Profil aktivieren: `docker compose --profile diffusion up -d automatic1111`.
4. **Worker überwachen**
   - `docker compose -f docker/compose.yaml logs -f shs-worker`.
   - Erwartete Logzeile: _"Detected service open-webui | Container ..."_ (siehe `SecureHomeSystem/Worker.cs`).
   - Prüfe die persistente Datei `${LOG_DIR:-../data/logs}/worker/worker-<Datum>.json` sowie die Cursor-Schnappschüsse unter `${LOG_DIR:-../data/logs}/state`. Bestätige, dass `${LOG_DIR:-../data/logs}/services/<service>.log` vorhanden ist und – falls größer oder älter – Rotation durch den Collector (`LogCollector:Rotation`) zu neuen Archivdateien führt.
5. **Health Checks bestätigen**
   - `curl http://localhost:5080/health` (Worker Health).
   - `curl http://localhost:3003/api/system/info` (OpenWebUI).
   - `curl -X POST http://localhost:7860/sdapi/v1/txt2img ...` (Stable Diffusion Smoke Test – Prompt steht in `SecureHomeSystem/Configuration/ServiceEndpointsOptions.cs`).
6. **Demo vorbereiten**
   - Tabs im Browser öffnen (OpenWebUI, AUTOMATIC1111, ggf. Dashboard).
   - Terminalfenster mit Worker-Logs bereit halten.
7. **Präsentation synchronisieren**
   - Offene Revision Flags auf Folie 14 aktualisieren (`docs/presentation/presentation-outline.md`).
   - Neue Screenshots gemäß Outline einfügen.

## 3. Demo-Skript (Moderationsleitfaden)

1. **Begrüßung & Vision (Slide 1)**
   - Stichworte aus Outline übernehmen.
   - Hinweis, dass Worker (siehe `SecureHomeSystem/Program.cs`) bewusst schlank gehalten ist.
2. **Architektur (Slides 4–6)**
   - Docker-Labels erläutern (`SecureHomeSystem/Configuration/DockerOptions.cs`).
   - Detection Loop zeigen (`SecureHomeSystem/Worker.cs` → `DetectDockerServicesAsync`).
3. **Live-Demo**
   - Worker-Log hervorheben: Container-ID, Image, Status (`SecureHomeSystem/Models/DetectedService.cs`).
   - OpenWebUI-Interaktion: Prompt generieren, Verweis auf GPU-Limits.
   - Optional: Diffusion-Profil starten, Wartezeit mit Hinweis auf `ServiceDetectionOptions.RetryCount` füllen.
4. **Risiken & Flags (Slide 14)**
   - Tests fehlen (`README.md`).
   - GPU-Automatisierung offen (`docs/operations/gpu-policy.md`).
5. **Roadmap & Call to Action**
   - Beitragspfad (`README.md` → Support & Contribution).
   - Testautomatisierung als nächster großer Schritt.

> 💡 **Moderations-Tipp:** Drucke dir diese Seite aus oder halte sie auf einem zweiten Monitor bereit. Jede Bullet verweist auf eine konkrete Datei, sodass Nachfragen sofort beantwortet werden können.

## 4. Kommunikations-Flags

| Flag | Bedeutung | Aktueller Plan |
| --- | --- | --- |
| 🟡 `testing-gap` | Keine automatisierten Tests. | Während Q&A proaktiv adressieren, Contribution-Aufruf wiederholen. |
| 🟠 `gpu-policy-automation` | Enforcement fehlt. | Manuellen Prozess im Runbook betonen, Verantwortliche im Betrieb nennen. |
| ??  `logging-analytics` | Zentrale Logs liegen als Dateien vor; Query- und Alerting-Layer fehlt. | Neue Ablage unter `${LOG_DIR}` hervorheben, integrierte Rotation (`LogCollector:Rotation`) dokumentieren und Anforderungen für Loki/Elastic sammeln. |
| 🔵 `documentation-sync` | Präsentationsinhalte ↔ README konsistent halten. | Nach jedem Merge `docs/presentation/presentation-outline.md` prüfen. |

Aktualisiere diese Tabelle, sobald ein Flag erledigt ist. Nutze farbige Emojis, damit der Status auf den ersten Blick erkennbar ist.

## 5. Post-Release Nachbereitung (T+1)

1. **Retro dokumentieren**
   - Kurzes Protokoll (Datum, Was lief gut, Was verbessern?) in `docs/operations/release-day-playbook.md` → Appendix B.
2. **Monitoring-Langläufer**
   - Worker-Logs weitere 24h beobachten (Automatisierung geplant, siehe `SecureHomeSystem/Services/DockerServiceDetector.cs`).
3. **Issue-Tracker aktualisieren**
   - Offene Flags anpassen, neue Tickets anlegen.
4. **Wissensweitergabe**
   - Session Recording teilen, Link im Repository-Wiki platzieren.

## Appendix A – Konfigurationsnotizen

> Trage hier spontane Anpassungen ein (Datum, was wurde geändert, warum?). Beispiel:
>
> - `2024-05-17` – `docker/.env`: Port 3003 → 13003, damit es auf Produktionsserver passt. Dokumentation in README angepasst.

## Appendix B – Retrospektive-Template

| Frage | Erkenntnisse |
| --- | --- |
| Was lief gut? | |
| Was war riskant? | |
| Welche Follow-ups haben wir erzeugt? | |

---

**Versionierung:** Aktualisiere die Kopfzeile bei jeder größeren Änderung (z. B. `v1.1 – 2024-05-17`). So erkennst du sofort, welche Fassung du zuletzt benutzt hast.
