# Operations Runbook

## Status Checks
- `docker compose -f docker/compose.yaml ls` shows the Compose project status.
- `docker ps --filter label=shs.role` lists managed containers and their state.
- `Get-Service Ollama` in PowerShell confirms the native Ollama service.

## Logs
- Worker: `docker compose -f docker/compose.yaml logs shs-worker`. ⚠️ Windows Event Log forwarding is **not** configured in the current worker build; revise this guidance if native Windows service hosting is introduced.
- OpenWebUI: `docker logs shs-stack-open-webui-1`.
- Stable Diffusion: `docker logs shs-stack-automatic1111-1`.

## Restart Policy
- Restart a single service: `docker compose -f docker/compose.yaml restart <service>`.
- Recycle the full stack: `docker compose -f docker/compose.yaml down` followed by `up -d`.
- GPU pressure: Pause heavy jobs by stopping the Stable Diffusion container (`docker compose stop automatic1111`); the worker registers configured limits but does not enforce them.

## Health Checks
- ⚠️ Worker HTTP health endpoint is **not yet implemented**. Replace this placeholder once a probe is shipped.
- OpenWebUI: `curl http://localhost:3003/api/system/info`.
- Qdrant: `curl http://localhost:6334/readyz`.
- AUTOMATIC1111: `curl -H "Content-Type: application/json" -X POST http://localhost:7860/sdapi/v1/txt2img -d '{"prompt":"test","steps":1,"width":64,"height":64}'`.

## Data Paths
- `docker/.env` defines host paths for volumes.
- Persistent data lives under `data/open-webui`, `data/qdrant`, `data/automatic1111`.
- Model cache resides in `models/stable-diffusion`.

## Common Issues
- **Permission denied** during `git add`: close Visual Studio or keep `.vs/` ignored.
- **GPU not visible**: run `nvidia-smi` and enable GPU support in Docker Desktop.
- **Service missing**: ensure the `shs.role` label exists and matches `Docker:Detection:LabelSelector`.
- **Docker CLI missing**: the worker requires access to the `docker` binary. When containerised, keep the `/var/run/docker.sock` mount present; when running natively, ensure Docker Desktop is installed and `docker` is on `PATH`.
- **AUTOMATIC1111 options API fails** (`sd_model_checkpoint` KeyError): the stack auto-runs `docker/automatic1111_patch.py` to coerce legacy images; rerun `docker compose up -d --force-recreate automatic1111` if a rebuilt container loses the patch.
