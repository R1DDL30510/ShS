# Docker-Compose-Einrichtung

## Voraussetzungen
- Docker Desktop mit WSL2-Backend und optional dem NVIDIA Container Toolkit für GPU-Workloads.
- Aktuelle NVIDIA-Treiber, wenn GPU-Beschleunigung benötigt wird.
- Ollama nativ auf dem Host installiert und auf Port `11434` laufend, wenn Modelle lokal bereitgestellt werden sollen.

## Schnellstart
```powershell
cd docker
# .env erstellen, falls Overrides (Pfade, Ports, GPU-Auswahl) erforderlich sind
docker compose --profile worker up -d --build
docker compose --profile diffusion up -d   # Stable Diffusion optional
```

## Dienste & Profile
- `shs-worker`: .NET-Orchestrierungs-Worker (Profil: `worker`). Enthält die Docker-CLI und nutzt das Mount `/var/run/docker.sock`, um mit dem Host-Daemon zu kommunizieren.
- `open-webui`, `qdrant`: Starten standardmäßig ohne Profil-Flag und sind mit `shs.role` gelabelt, damit der Worker sie erkennt. `open-webui` wird in Logs als HomeChatGPT ausgegeben.
- `automatic1111`: Startet nur, wenn das Profil `diffusion` angegeben wird. GPU-Zugriff erfolgt über den Eintrag `deploy.resources.reservations.devices` sowie die Umgebungsvariablen `NVIDIA_VISIBLE_DEVICES`/`AUTOMATIC1111_GPU_*`. Der Container führt vor dem Start von Stable Diffusion `docker/automatic1111_patch.py` aus.

## GPU-Limits
- Standard `OLLAMA_MAX_GPU_MEMORY=0.8` (80 % Zielauslastung), konfigurierbar über `.env`.
- Stable Diffusion startet mit `--medvram` und `--opt-sdp-attention`; erweitern Sie die Flags über `AUTOMATIC1111_ARGS`.
- Die GPU-Auswahl ist standardmäßig auf `all` gesetzt (`AUTOMATIC1111_GPU_SELECTION`/`AUTOMATIC1111_GPU_COUNT`) und kann hostabhängig eingeschränkt werden.
- Der Worker protokolliert konfigurierte GPU-Schutzmaßnahmen lediglich zur Transparenz; eine Durchsetzung existiert noch nicht.

> ⚠️ **Revisionshinweis:** Aktualisieren Sie diesen Abschnitt, sobald GPU-Durchsetzung oder alternative Profile eingeführt werden.

## Volumes
- Daten-Root: `../data` relativ zum Repository (`open-webui`, `qdrant`, `automatic1111`).
- Modelle: `../models/stable-diffusion` enthält AUTOMATIC1111-Checkpoints.
- Workspace: `../stablediff/stable-diffusion-webui` (falls vorhanden) wird schreibgeschützt für Patch-Skripte eingebunden.

## Health Checks
- Containerzustände mit `docker compose ps` prüfen.
- HomeChatGPT (OpenWebUI): `curl http://localhost:3003/api/system/info`.
- Qdrant: `curl http://localhost:6334/readyz`.
- AUTOMATIC1111 (wenn das Profil `diffusion` aktiv ist): `curl -X POST http://localhost:7860/sdapi/v1/txt2img -d '{"prompt":"test","steps":1,"width":64,"height":64}'`.
- Der Worker protokolliert alle 30 Sekunden erkannte Dienste (`docker compose logs shs-worker`).

## Kontrollpunkte
1. **Checkpoint 1 – Ollama/HomeChatGPT (OpenWebUI)**
   - `docker compose --profile worker up -d --build`
   - Prüfen Sie, dass `shs-stack-open-webui-1` (HomeChatGPT) unter `http://localhost:3003` im Zustand „healthy“ ist und `shs-stack-qdrant-1` `http://localhost:6334/readyz` bedient.
   - Führen Sie `dotnet run --project SecureHomeSystem` (oder den Worker-Container) aus, um zu bestätigen, dass die Logs beide Dienste anzeigen.
2. **Checkpoint 2 – Stable Diffusion bereit**
   - `docker compose --profile worker --profile diffusion up -d --build`
   - Sicherstellen, dass GPUs für Docker Desktop verfügbar sind (`nvidia-smi` innerhalb von WSL).
   - Bestätigen Sie, dass `shs-stack-automatic1111-1` auf `http://localhost:7860/sdapi/v1/txt2img` reagiert.
   - Überprüfen Sie die Worker-Logs auf einen erkannten Eintrag `stable-diffusion`, bevor Sie den Baseline-Stand kennzeichnen.
   - Compose injiziert beim Containerstart `docker/automatic1111_patch.py`, um Legacy-AUTOMATIC1111-Builds zu umgehen, die `sd_model_checkpoint` in `/sdapi/v1/options` auslassen.
