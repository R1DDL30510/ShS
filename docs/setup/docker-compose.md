# Docker Compose Setup

## Prerequisites
- Docker Desktop with WSL2 backend and (optionally) the NVIDIA Container Toolkit for GPU workloads.
- Recent NVIDIA drivers when GPU acceleration is required.
- Ollama installed natively on the host and running on port `11434` when models should be served locally.

## Quick Start
```powershell
cd docker
# create .env if overrides (paths, ports, GPU selection) are needed
docker compose --profile worker up -d --build
docker compose --profile diffusion up -d   # Stable Diffusion optional
```

## Services & Profiles
- `shs-worker`: .NET orchestration worker (profile: `worker`). Bundles the Docker CLI and uses the `/var/run/docker.sock` mount to talk to the host daemon.
- `open-webui`, `qdrant`: Start by default with no profile flag and are labelled with `shs.role` for worker detection.
- `automatic1111`: Only starts when the `diffusion` profile is supplied. GPU access is exposed through the `deploy.resources.reservations.devices` entry and the `NVIDIA_VISIBLE_DEVICES`/`AUTOMATIC1111_GPU_*` environment variables. The container runs `docker/automatic1111_patch.py` before launching Stable Diffusion.

## GPU Limits
- Default `OLLAMA_MAX_GPU_MEMORY=0.8` (80% target load), configurable via `.env`.
- Stable Diffusion launches with `--medvram` and `--opt-sdp-attention`; extend the flags through `AUTOMATIC1111_ARGS`.
- GPU selection defaults to `all` through `AUTOMATIC1111_GPU_SELECTION`/`AUTOMATIC1111_GPU_COUNT` and can be narrowed per host.
- The worker records configured GPU guardrails for awareness only; enforcement is not yet implemented.

> ⚠️ **Revision Flag:** Update this section once GPU enforcement or alternative profiles are introduced.

## Volumes
- Data root: `../data` relative to the repo (`open-webui`, `qdrant`, `automatic1111`).
- Models: `../models/stable-diffusion` holds AUTOMATIC1111 checkpoints.
- Workspace: `../stablediff/stable-diffusion-webui` (when present) is mounted read-only for patch scripts.

## Health Checks
- Worker: `curl http://localhost:${WORKER_HEALTH_PORT:-5080}/health`.
- Check container states with `docker compose ps`.
- OpenWebUI: `curl http://localhost:3003/api/system/info`.
- Qdrant: `curl http://localhost:6334/readyz`.
- AUTOMATIC1111 (when the `diffusion` profile is active): `curl -X POST http://localhost:7860/sdapi/v1/txt2img -d '{"prompt":"test","steps":1,"width":64,"height":64}'`.
- The worker logs detected services every 30 seconds (`docker compose logs shs-worker`).

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
