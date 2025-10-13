# Docker Compose Setup

## Prerequisites
- Docker Desktop with WSL2 backend and NVIDIA Container Toolkit.
- Latest NVIDIA driver exposing both 12 GB GPUs to Docker.
- Ollama installed native on Windows and running as a service on port `11434`.

## Schnellstart
```powershell
cd docker
Copy-Item .env.example .env
docker compose --profile worker up -d
docker compose --profile diffusion up -d   # Stable Diffusion optional
```

## Dienste & Profile
- `shs-worker`: .NET Worker-Orchestrator (`worker` Profil).
- `open-webui`, `qdrant`: Standardprofile, werden immer gestartet.
- `automatic1111`: Nur mit Profil `diffusion`; GPU-Zugriff via `gpus: all`.

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
  - OpenWebUI via `GET /api/system/info`
  - Qdrant via `GET /readyz`
  - AUTOMATIC1111 via a low-cost `/sdapi/v1/txt2img` request
- Failures trigger automatic restart attempts and pause the job queue.
