# Docker Compose Setup

## Prerequisites
- Docker Desktop with WSL2 backend and NVIDIA Container Toolkit.
- Latest NVIDIA driver exposing both 12 GB GPUs to Docker.
- Ollama installed native on Windows and running as a service on port `11434`.

## Schnellstart
```powershell
cd docker
Copy-Item .env.example .env
docker compose --profile worker up -d --build
docker compose --profile diffusion up -d   # Stable Diffusion optional
```

## Dienste & Profile
- `shs-worker`: .NET Worker-Orchestrator (`worker` Profil). Enthält die Docker CLI und greift über das gemountete `/var/run/docker.sock` auf den Host-Daemon zu.
- `open-webui`, `qdrant`: Standardprofile, werden immer gestartet.
- `automatic1111`: Nur mit Profil `diffusion`; GPU-Zugriff via `gpus: all`. Nutzt das Docker-Hub-Image `sdwebui/stable-diffusion-webui:latest` (~16 GB Download inklusive CUDA/PyTorch).

## GPU Limits
- Default `OLLAMA_MAX_GPU_MEMORY=0.8` (80% target load).
- Stable Diffusion uses `--medvram` and `--opt-sdp-attention` to reduce VRAM pressure.
- Worker scheduler queues heavy tasks whenever GPU utilisation exceeds 50%.

## Volumes
- Data root: `../data` relative to the repo (`open-webui`, `qdrant`, `automatic1111`).
- Models: `../models/stable-diffusion` holds AUTOMATIC1111 checkpoints.
- Workspace: `../stablediff/stable-diffusion-webui` mounted read-only for scripts.

## Health Checks
- Immediately after startup the worker runs smoke tests:
  - OpenWebUI via `GET http://localhost:3003/api/system/info`
  - Qdrant via `GET http://localhost:6334/readyz`
  - AUTOMATIC1111 via a low-cost `/sdapi/v1/txt2img` request
- Failures trigger automatic restart attempts and pause the job queue.

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
