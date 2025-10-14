# Operations Runbook

## Status Checks
- `docker compose -f docker/compose.yaml ls` shows the Compose project status.
- `docker ps --filter label=shs.role` lists managed containers and their state.
- `Get-Service Ollama` in PowerShell confirms the native Ollama service.

## Logs
- Worker (live stream): `docker compose -f docker/compose.yaml logs shs-worker`.
- Worker (persistent JSON): `${LOG_DIR:-../data/logs}/worker/worker-<date>.json` (PowerShell helper: `Get-Content -Path (Join-Path $PWD '..\data\logs\worker\worker-*.json') -Wait` when using the default path).
- Service archives: `${LOG_DIR:-../data/logs}/services/<service>.log` holds the latest harvested stdout per container. Files rotate automatically at ~10 MB or 7 days and keep five timestamped archives by default (`LogCollector:Rotation` in `appsettings.json` controls the limits).
- OpenWebUI (live): `docker logs shs-stack-open-webui-1`.
- Stable Diffusion (live): `docker logs shs-stack-automatic1111-1`.

## Restart Policy
- Restart a single service: `docker compose -f docker/compose.yaml restart <service>`.
- Recycle the full stack: `docker compose -f docker/compose.yaml down` followed by `up -d`.
- GPU pressure: Pause heavy jobs by stopping the Stable Diffusion container (`docker compose stop automatic1111`); the worker registers configured limits but does not enforce them.

## Health Checks
- Worker: `curl http://localhost:${WORKER_HEALTH_PORT:-5080}/health` for HTTP 200 or `curl http://localhost:${WORKER_HEALTH_PORT:-5080}/live` for a minimal JSON liveness response.
- OpenWebUI: `curl http://localhost:3003/api/system/info`.
- Qdrant: `curl http://localhost:6334/readyz`.
- AUTOMATIC1111: `curl -H "Content-Type: application/json" -X POST http://localhost:7860/sdapi/v1/txt2img -d '{"prompt":"test","steps":1,"width":64,"height":64}'`.

## Data Paths
- `docker/.env` defines host paths for volumes.
- Persistent data lives under `data/open-webui`, `data/qdrant`, `data/automatic1111`.
- Model cache resides in `models/stable-diffusion`.
- Logs persist under `${LOG_DIR:-../data/logs}` with subfolders `worker`, `services`, and `state`.

## Common Issues
- **Permission denied** during `git add`: close Visual Studio or keep `.vs/` ignored.
- **GPU not visible**: run `nvidia-smi` and enable GPU support in Docker Desktop.
- **Service missing**: ensure the `shs.role` label exists and matches `Docker:Detection:LabelSelector`.
- **Docker CLI missing**: the worker requires access to the `docker` binary. When containerised, keep the `/var/run/docker.sock` mount present; when running natively, ensure Docker Desktop is installed and `docker` is on `PATH`.
- **AUTOMATIC1111 options API fails** (`sd_model_checkpoint` KeyError): the stack auto-runs `docker/automatic1111_patch.py` to coerce legacy images; rerun `docker compose up -d --force-recreate automatic1111` if a rebuilt container loses the patch.
