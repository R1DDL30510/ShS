# Docker-Compose-Setup

## Voraussetzungen
- Docker Desktop mit WSL2-Backend und (optional) dem NVIDIA Container Toolkit für GPU-Workloads.
- Aktuelle NVIDIA-Treiber, wenn GPU-Beschleunigung benötigt wird.
- Ollama nativ auf dem Host installiert und auf Port `11434` laufend, wenn Modelle lokal bereitgestellt werden sollen.

## Quickstart
```powershell
cd docker
# .env anlegen, falls Overrides (Pfade, Ports, GPU-Auswahl, Log-Speicher) nötig sind
docker compose --profile worker up -d --build
docker compose --profile diffusion up -d   # Stable Diffusion optional
```

## Services & Profile
- `shs-worker`: .NET-Orchestrierungs-Worker (Profil: `worker`). Enthält die Docker-CLI und nutzt den Mount `/var/run/docker.sock`, um mit dem Host-Daemon zu sprechen.
- `open-webui`, `qdrant`: Starten standardmäßig ohne Profil-Flag und tragen `shs.role`-Labels für die Worker-Erkennung.
- `automatic1111`: Startet nur mit dem Profil `diffusion`. GPU-Zugriff wird über `deploy.resources.reservations.devices` sowie die Variablen `NVIDIA_VISIBLE_DEVICES`/`AUTOMATIC1111_GPU_*` bereitgestellt. Der Container führt vor dem Start von Stable Diffusion `docker/automatic1111_patch.py` aus.

## GPU-Limits
- Standard `OLLAMA_MAX_GPU_MEMORY=0.8` (80 % Zielauslastung), konfigurierbar über `.env`.
- Stable Diffusion startet mit `--medvram` und `--opt-sdp-attention`; zusätzliche Flags lassen sich über `AUTOMATIC1111_ARGS` ergänzen.
- GPU-Auswahl standardmäßig `all` via `AUTOMATIC1111_GPU_SELECTION`/`AUTOMATIC1111_GPU_COUNT`, anpassbar pro Host.
- Der Worker protokolliert konfigurierte GPU-Geländer nur zur Transparenz; eine Durchsetzung ist noch nicht implementiert.

> ⚠️ **Revisionsflag:** Aktualisiere diesen Abschnitt, sobald GPU-Enforcement oder alternative Profile eingeführt werden.

## Volumes
- Daten-Root: `../data` relativ zum Repository (`open-webui`, `qdrant`, `automatic1111`).
- Logs: `${LOG_DIR:-../data/logs}` auf dem Host wird nach `/logs` gemountet und enthält Worker-JSONs sowie geharvestete Container-Logs (Rotationseinstellungen unter `LogCollector:Rotation`).
- Modelle: `../models/stable-diffusion` hält AUTOMATIC1111-Checkpoints bereit.
- Workspace: `../stablediff/stable-diffusion-webui` (falls vorhanden) wird schreibgeschützt für Patch-Skripte eingebunden.

## Health Checks
- Worker: `curl http://localhost:${WORKER_HEALTH_PORT:-5080}/health`.
- Containerzustände mit `docker compose ps` prüfen.
- OpenWebUI: `curl http://localhost:3003/api/system/info`.
- Qdrant: `curl http://localhost:6334/readyz`.
- AUTOMATIC1111 (bei aktivem `diffusion`-Profil): `curl -X POST http://localhost:7860/sdapi/v1/txt2img -d '{"prompt":"test","steps":1,"width":64,"height":64}'`.
- Der Worker protokolliert erkannte Dienste alle 30 Sekunden (`docker compose logs shs-worker`) und persistiert strukturierte Einträge unter `/logs/worker` im Container (`LOG_DIR` auf dem Host).

## Checkpoints
1. **Checkpoint 1 – Ollama/OpenWebUI**
   - `docker compose --profile worker up -d --build`
   - Verifiziere, dass `shs-stack-open-webui-1` auf `http://localhost:3003` erreichbar ist und `shs-stack-qdrant-1` `http://localhost:6334/readyz` bedient.
   - Führe `dotnet run --project SecureHomeSystem` (oder den Worker-Container) aus und prüfe, dass die Detection-Logs beide Dienste auflisten.
2. **Checkpoint 2 – Stable Diffusion bereit**
   - `docker compose --profile worker --profile diffusion up -d --build`
   - Sicherstellen, dass GPUs in Docker Desktop verfügbar sind (`nvidia-smi` innerhalb von WSL).
   - Bestätigen, dass `shs-stack-automatic1111-1` auf `http://localhost:7860/sdapi/v1/txt2img` antwortet.
   - Worker-Logs auf einen erkannten `stable-diffusion`-Eintrag prüfen, bevor der Baseline-Tag erfolgt.
   - Compose führt `docker/automatic1111_patch.py` beim Containerstart aus, um ältere AUTOMATIC1111-Builds zu korrigieren, denen `sd_model_checkpoint` unter `/sdapi/v1/options` fehlt.
