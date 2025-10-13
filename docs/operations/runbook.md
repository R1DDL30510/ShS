# Operations Runbook

## Status Checks
- `docker compose -f docker/compose.yaml ls` shows the Compose project status.
- `docker ps --filter label=shs.role` lists managed containers and their state.
- `Get-Service Ollama` in PowerShell confirms the native Ollama service.

## Logs
- Worker: `docker logs shs-worker-1` or Windows Event Log (`Applications and Services Logs/SecureHomeSystem`).
- OpenWebUI: `docker logs shs-stack-open-webui-1`.
- Stable Diffusion: `docker logs shs-stack-automatic1111-1`.

## Restart Policy
- Restart a single service: `docker compose -f docker/compose.yaml restart <service>`.
- Recycle the full stack: `docker compose -f docker/compose.yaml down` followed by `up -d`.
- GPU pressure: Worker pauses new jobs; optionally free capacity with `docker compose stop automatic1111`.

## Health Checks
- Worker endpoint (planned): `http://localhost:5169/health`.
- OpenWebUI: `curl http://localhost:3000/api/system/info`.
- Qdrant: `curl http://localhost:6333/readyz`.
- AUTOMATIC1111: `curl -X POST http://localhost:7860/sdapi/v1/txt2img -d '{"prompt":"test","steps":1,"width":64,"height":64}'`.

## Data Paths
- `docker/.env` defines host paths for volumes.
- Persistent data lives under `data/open-webui`, `data/qdrant`, `data/automatic1111`.
- Model cache resides in `models/stable-diffusion`.

## Common Issues
- **Permission denied** during `git add`: close Visual Studio or keep `.vs/` ignored.
- **GPU not visible**: run `nvidia-smi` and enable GPU support in Docker Desktop.
- **Service missing**: ensure the `shs.role` label exists and matches `Docker:Detection:LabelSelector`.
