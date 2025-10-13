# Operations Runbook

## Verifying Service State
- List running containers for this stack:
  ```sh
  docker ps --filter label=com.docker.compose.project=shs-stack
  ```
- Inspect a specific service (e.g., Open WebUI):
  ```sh
  docker inspect open-webui --format 'Status={{.State.Status}} Image={{.Config.Image}}'
  ```
- Confirm volumes:
  - Open WebUI data: `../data/open-webui`
  - Qdrant storage: `../data/qdrant`
  - Automatic1111 outputs: `../data/automatic1111`
  - Automatic1111 models: `../models/stable-diffusion`

## Health Checks
- Open WebUI: `GET http://localhost:${WEBUI_PORT:-3000}/health`
- Qdrant: `GET http://localhost:${QDRANT_PORT:-6333}/healthz`
- Automatic1111: `GET http://localhost:${AUTOMATIC1111_PORT:-7860}/sdapi/v1/sd-models`
- Ollama (native service): `GET http://localhost:11434/api/tags`

## GPU Guardrails
- The worker monitors GPU utilisation using NVML and queues heavy tasks whenever utilisation exceeds 50%.
- GPU memory usage is capped at roughly 80% by combining container arguments (`--medvram`, `OLLAMA_MAX_GPU_MEMORY`) with scheduler enforcement.
- If both GPUs exceed the utilisation threshold, new Stable Diffusion jobs remain queued until utilisation drops.

## Restarting Services
- Single service:
  ```sh
  docker compose -f docker/compose.yaml restart open-webui
  ```
- Entire stack:
  ```sh
  docker compose -f docker/compose.yaml down
  docker compose -f docker/compose.yaml up -d
  ```

## Logs
- Follow worker logs:
  ```sh
  docker compose -f docker/compose.yaml logs -f shs-worker
  ```
- Check Open WebUI logs for prompt execution details:
  ```sh
  docker compose -f docker/compose.yaml logs -f open-webui
  ```

## Investigating Failures
- Review recent worker warnings for health-check failures in `logs/worker/*.log` (if configured) or container logs.
- Use `docker inspect --format '{{json .State.Health}}' <container>` for detailed health probe status.
- If GPU saturation persists, ensure no stray processes run on the host by checking `nvidia-smi`.
