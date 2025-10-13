# Docker Compose Setup

## Prerequisites
- Docker Desktop with WSL2 backend and (optionally) the NVIDIA Container Toolkit for GPU workloads.
- Recent NVIDIA drivers when GPU acceleration is required.
- Ollama installed natively on the host and running on port `11434` when models should be served locally.

## Schnellstart
```powershell
cd docker
# create .env if overrides (paths, ports, GPU selection) are needed
docker compose --profile worker up -d --build
docker compose --profile diffusion up -d   # Stable Diffusion optional
```

## Dienste & Profile
- `shs-worker`: .NET Worker-Orchestrator (`worker` Profil). Enthält die Docker CLI und greift über das gemountete `/var/run/docker.sock` auf den Host-Daemon zu.
- `open-webui`, `qdrant`: Werden standardmäßig gestartet und sind mit `shs.role`-Labels für die Worker-Erkennung versehen.
- `automatic1111`: Nur mit Profil `diffusion`; GPU-Zugriff via `gpus: all`. Nutzt das Docker-Hub-Image `sdwebui/stable-diffusion-webui:latest` und führt beim Start `docker/automatic1111_patch.py` aus.

## GPU Limits
- Default `OLLAMA_MAX_GPU_MEMORY=0.8` (80% target load), konfigurierbar über `.env`.
- Stable Diffusion nutzt standardmäßig `--medvram` und `--opt-sdp-attention`, weitere Argumente können über `AUTOMATIC1111_ARGS` gesetzt werden.
- Der Worker protokolliert die konfigurierten GPU-Grenzwerte; eine automatische Durchsetzung ist noch nicht implementiert.

## Volumes
- Data root: `../data` relative to the repo (`open-webui`, `qdrant`, `automatic1111`).
- Models: `../models/stable-diffusion` holds AUTOMATIC1111 checkpoints.
- Workspace: `../stablediff/stable-diffusion-webui` (falls vorhanden) wird schreibgeschützt für Patch-Skripte eingebunden.

## Health Checks
- Überprüfen Sie die Containerzustände mit `docker compose ps`.
- OpenWebUI: `curl http://localhost:3003/api/system/info`.
- Qdrant: `curl http://localhost:6334/readyz`.
- AUTOMATIC1111 (falls Profil `diffusion` aktiv ist): `curl -X POST http://localhost:7860/sdapi/v1/txt2img -d '{"prompt":"test","steps":1,"width":64,"height":64}'`.
- Der Worker protokolliert erkannte Services alle 30 Sekunden (`docker compose logs shs-worker`).

## Checkpoints
1. **Checkpoint 1 – Ollama/OpenWebUI**  
   - `docker compose --profile worker up -d --build`  
   - Verify `shs-stack-open-webui-1` is healthy on `http://localhost:3003` and `shs-stack-qdrant-1` is serving `http://localhost:6334/readyz`.  
   - Run `dotnet run --project SecureHomeSystem` (or the worker container) to confirm detection logs show both services.
2. **Checkpoint 2 – Stable Diffusion Ready**  
   - `docker compose --profile worker --profile diffusion up -d --build`  
   - Ensure GPUs are available to Docker Desktop (`nvidia-smi` inside WSL).  
   - Confirm `shs-stack-automatic1111-1` responds to `http://localhost:7860/sdapi/v1/txt2img`.  
   - Review worker logs for a detected `stable-diffusion` entry before tagging the baseline.  
   - Compose injects `docker/automatic1111_patch.py` at container start to work around legacy AUTOMATIC1111 builds that omit `sd_model_checkpoint` in `/sdapi/v1/options`.
