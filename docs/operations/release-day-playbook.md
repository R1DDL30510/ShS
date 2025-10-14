# Releasetag-Playbook

> **Zielgruppe:** Du selbst in ein paar Monaten – oder Kolleg:innen, die kurzfristig einspringen müssen. Dieses Playbook fasst alle relevanten Schritte zusammen, damit der SecureHomeSystem-Releasetag reibungslos abläuft und jede Entscheidung nachvollziehbar bleibt.

## 1. Vorbereitungs-Timeline (T-7 bis T-1)

| Zeitpunkt | Aufgabe | Referenzen |
| --- | --- | --- |
| T-7 | Projektüberblick auffrischen, Präsentationsleitfaden mit Stakeholdern abgleichen. | `docs/presentation/presentation-outline.md` (Folien 1–5) |
| T-5 | Compose-Stack lokal durchstarten, offene Labels prüfen. | `docker/compose.yaml`, `SecureHomeSystem/Configuration/DockerOptions.cs` |
| T-4 | GPU-Richtlinie verifizieren, ggf. Limits aktualisieren. | `docs/operations/gpu-policy.md`, `SecureHomeSystem/Configuration/ResourceSchedulerOptions.cs` |
| T-3 | Demo-Skript entwerfen und Worker-Logs mitschneiden. | `SecureHomeSystem/Worker.cs`, `docs/operations/runbook.md` |
| T-2 | Revisionshinweise evaluieren und Status in Präsentation aufnehmen. | `README.md` → Abschnitt **Revisionshinweise** |
| T-1 | End-to-End-Probelauf inkl. Erstellung der Screenshots. | `docs/presentation/presentation-outline.md` → Screenshot-Hinweise |

> 🔁 **Wiederholbarkeit:** Jede Aufgabe nennt die Dateien, die du im Blick behalten musst. Wenn Konfigurationswerte verändert werden, dokumentiere Datum & Grund direkt in diesem Playbook unter „Anhang A“.

## 2. Checkliste am Releasetag

1. **Repository-Stand verifizieren**
   - `git status` – Branch und Sauberkeit prüfen.
   - `git log -5 --oneline` – Überblick über die letzten Commits behalten.
2. **Umgebung vorbereiten**
   - `cp docker/.env.example docker/.env` (falls System frisch aufgesetzt).
   - GPU-Verfügbarkeit prüfen: `nvidia-smi`.
3. **Stack starten**
   - `docker compose -f docker/compose.yaml up -d --build`.
   - Optionales Diffusion-Profil aktivieren: `docker compose --profile diffusion up -d automatic1111`.
4. **Worker überwachen**
   - `docker compose -f docker/compose.yaml logs -f shs-worker`.
   - Erwartete Logzeile: _"Dienst erkannt HomeChatGPT | Container ..."_ (siehe `SecureHomeSystem/Worker.cs`).
5. **Health Checks bestätigen**
   - `curl http://localhost:3003/api/system/info` (HomeChatGPT/OpenWebUI).
   - `curl -X POST http://localhost:7860/sdapi/v1/txt2img ...` (Stable-Diffusion-Smoke-Test – Prompt steht in `SecureHomeSystem/Configuration/ServiceEndpointsOptions.cs`).
6. **Demo vorbereiten**
   - Browser-Tabs öffnen (HomeChatGPT/OpenWebUI, AUTOMATIC1111, ggf. Dashboard).
   - Terminalfenster mit Worker-Logs bereithalten.
7. **Präsentation synchronisieren**
   - Offene Revisionshinweise auf Folie 14 aktualisieren (`docs/presentation/presentation-outline.md`).
   - Neue Screenshots gemäß Leitfaden einfügen.

## 3. Demo-Skript (Moderationsleitfaden)

1. **Begrüßung & Vision (Folie 1)**
   - Stichworte aus dem Leitfaden übernehmen.
   - Hinweis, dass der Worker (`SecureHomeSystem/Program.cs`) bewusst schlank gehalten ist.
2. **Architektur (Folien 4–6)**
   - Docker-Labels erläutern (`SecureHomeSystem/Configuration/DockerOptions.cs`).
   - Detection Loop zeigen (`SecureHomeSystem/Worker.cs` → `DetectDockerServicesAsync`).
3. **Live-Demo**
  - Worker-Log hervorheben: Container-ID, Image, Status (`SecureHomeSystem/Models/DetectedService.cs`).
  - HomeChatGPT-Interaktion: Prompt generieren, Verweis auf GPU-Limits.
  - Optional: Diffusionsprofil starten, Wartezeit mit Hinweis auf `ServiceDetectionOptions.RetryCount` überbrücken.
4. **Risiken & Flags (Folie 14)**
  - Health Endpoint fehlt (`docs/operations/runbook.md`).
  - Tests fehlen (`README.md`).
  - GPU-Automatisierung offen (`docs/operations/gpu-policy.md`).
5. **Roadmap & Call to Action**
  - Beitragspfad (`README.md` → Support & Contribution).
  - Testautomatisierung als nächster großer Schritt.

> 💡 **Moderationstipp:** Drucke dir diese Seite aus oder halte sie auf einem zweiten Monitor bereit. Jede Bullet verweist auf eine konkrete Datei, sodass Nachfragen sofort beantwortet werden können.

## 4. Kommunikations-Flags

| Flag | Bedeutung | Aktueller Plan |
| --- | --- | --- |
| 🟡 `worker-health-endpoint` | HTTP-Health-Check fehlt. | Backlog-Issue #12, auf Folie 14 erwähnen, keine Live-Demo geplant. |
| 🟡 `testing-gap` | Keine automatisierten Tests. | Während der Q&A proaktiv adressieren, Contribution-Aufruf wiederholen. |
| 🟠 `gpu-policy-automation` | Durchsetzung fehlt. | Manuellen Prozess im Runbook betonen, Verantwortliche im Betrieb benennen. |
| 🔵 `documentation-sync` | Präsentationsinhalte ↔ README konsistent halten. | Nach jedem Merge `docs/presentation/presentation-outline.md` prüfen. |

Aktualisiere diese Tabelle, sobald ein Flag erledigt ist. Nutze farbige Emojis, damit der Status auf den ersten Blick erkennbar ist.

## 5. Nachbereitung (T+1)

1. **Retro dokumentieren**
   - Kurzes Protokoll (Datum, Was lief gut, Was verbessern?) in `docs/operations/release-day-playbook.md` → Anhang B.
2. **Monitoring fortführen**
   - Worker-Logs weitere 24 Stunden beobachten (Automatisierung geplant, siehe `SecureHomeSystem/Services/DockerServiceDetector.cs`).
3. **Issue-Tracker aktualisieren**
   - Offene Flags anpassen, neue Tickets anlegen.
4. **Wissensweitergabe**
   - Session-Aufzeichnung teilen, Link im Repository-Wiki platzieren.

## Anhang A – Konfigurationsnotizen

> Trage hier spontane Anpassungen ein (Datum, was wurde geändert, warum?). Beispiel:
>
> - `2024-05-17` – `docker/.env`: Port 3003 → 13003, damit es auf den Produktionsserver passt. Dokumentation in README angepasst.

## Anhang B – Retrospektive-Vorlage

| Frage | Erkenntnisse |
| --- | --- |
| Was lief gut? | |
| Was war riskant? | |
| Welche Follow-ups haben wir erzeugt? | |

---

**Versionierung:** Aktualisiere die Kopfzeile bei jeder größeren Änderung (z. B. `v1.1 – 2024-05-17`). So erkennst du sofort, welche Fassung du zuletzt verwendet hast.
