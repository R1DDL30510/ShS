# SecureHomeSystem Stack

## Overview
SecureHomeSystem is a .NET 9 worker service that supervises an AI-assisted home automation stack. The worker loads runtime settings from `appsettings.json`, registers Docker and service endpoint options, and runs a hosted background service for continuous orchestration tasks. The `Worker` service logs configured endpoints, repeatedly triggers Docker discovery, and reports the status of managed containers every 30 seconds.

## Stack Architecture
The project combines a lightweight orchestration worker with a Docker Compose workload containing generative AI tooling and supporting services:

| Component | Description |
| --- | --- |
| `shs-worker` | Containerised build of the .NET worker with access to the host Docker socket for runtime detection and orchestration. |
| `open-webui` | OpenWebUI front-end for Ollama, configured through environment variables and labelled for detection by the worker. |
| `qdrant` | Vector database used by OpenWebUI for retrieval-augmented workflows. |
| `automatic1111` | Stable Diffusion web UI container with GPU reservations, model mounts, and startup patching for compatibility tweaks. |

Compose labels (`shs.role`) allow the worker to correlate running containers with logical services when parsing `docker ps` output.

## Key Capabilities
- **Docker-aware orchestration** - Detects labelled containers via the Docker CLI, surfaces status telemetry, and gracefully handles CLI failures or cancellation.
- **Configurable service endpoints** - Centralised options objects define base URLs, GPU thresholds, and health probes for Ollama, OpenWebUI, and Stable Diffusion services.
- **HTTP health endpoints** - `/health` and `/live` routes expose the worker's status for Compose health checks and external monitoring.
- **Centralised log capture** - Serilog records worker events as JSON under `/logs/worker` while a background collector persists container stdout streams per service in `/logs/services`, anchored by cursor files in `/logs/state`.
- **GPU policy configuration** - Resource scheduler thresholds are captured in configuration for future automation while operations teams continue to enforce limits manually.
- **Operational guidance** - Runbooks document log locations, restart procedures, health checks, and escalation paths for production operations.

## Prerequisites
- [.NET SDK 9.0](https://dotnet.microsoft.com/) for local builds of the worker service.
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) with Compose V2 and GPU passthrough (NVIDIA) enabled.
- NVIDIA drivers plus `nvidia-smi` availability for GPU telemetry if Stable Diffusion or Ollama leverage GPU acceleration.

## Getting Started
1. Clone the repository and create a `docker/.env` file (see `docker/compose.yaml` for supported overrides) so host-specific paths and GPU settings can be supplied.
2. Build and start the stack:
   ```bash
   docker compose -f docker/compose.yaml up -d --build
   ```
3. Follow logs for initial detection:
   ```bash
   docker compose -f docker/compose.yaml logs -f shs-worker
   ```
4. Access OpenWebUI at `http://localhost:3003` (unless overridden) and verify Stable Diffusion readiness at `http://localhost:7860`.

The worker reports each detected service with its container ID, image, status, and running flag, providing quick validation of stack health.

## Configuration
Runtime settings live in `SecureHomeSystem/appsettings.json` and can be overridden via environment variables when containerised. Key sections include:
- `Docker`: Compose file path, project name, auto-start profiles, and detection label selectors.
- `Services`: Endpoint metadata for Ollama, OpenWebUI, and Stable Diffusion, including health check paths and GPU memory caps.
- `ResourceScheduler`: GPU utilisation thresholds and polling cadence for future scheduling features.
- `Health`: Port and path configuration for the worker’s `/health` and `/live` endpoints.

Refer to `docs/reference/configuration.md` for the full parameter reference and defaults, including the nested detection settings under `Docker:Detection` and service endpoint overrides.

## Logging & Observability
- Worker telemetry is stored as rolling JSON files at `/logs/worker/worker-<date>.json`. Files rotate daily (or at 10 MB) and retain the last 14 segments by default.
- Container stdout is captured by `ContainerLogCollector` into `/logs/services/<service>.log`, with per-service cursors persisted under `/logs/state/<service>.cursor` to avoid duplicates across restarts. Logs rotate in-place when they reach 10 MB or 7 days, and the worker keeps up to five archives per service.
- Override the host-side mount with the `LOG_DIR` environment variable (default `../data/logs`) in `docker/.env`; the Compose file binds this directory into the worker container as `/logs`.
- Quick triage still benefits from `docker compose -f docker/compose.yaml logs -f shs-worker`, but use the persisted files for audit trails, comparisons across restarts, and feeding future automated tests.

## Operations
- **Status checks:** `docker compose -f docker/compose.yaml ls` and `docker ps --filter label=shs.role` to confirm container health.
- **Worker health:** `curl http://localhost:5080/health` (customise with `WORKER_HEALTH_PORT`) for an HTTP 200 response.
- **Logs:** Tail `/logs/worker/worker-*.json` for structured worker events or inspect `/logs/services/<service>.log` for container stdout snapshots; use `docker compose -f docker/compose.yaml logs` for live streaming when needed.
- **Restarts:** Target a single service with `docker compose restart <service>` or recycle the full stack with `down`/`up -d`.
- **GPU management:** Follow the GPU policy guidance to maintain VRAM caps and queue behaviour during contention.
- **Release readiness:** Consult `docs/operations/release-day-playbook.md` for the day-of checklist, demo script, and revision
  flags you should highlight to stakeholders.

## Development Workflow
- Run the worker locally with `dotnet run --project SecureHomeSystem` to iterate without containers.
- Execute unit or integration tests when they are introduced; **no automated test suite ships with the repository yet.**
- Container builds install the Docker CLI within the runtime image so the worker can communicate with the host daemon when deployed in Compose.

## Repository Structure
```
SecureHomeSystem/      # .NET worker service source, options, and hosted worker
  Configuration/      # Strongly typed options for Docker, services, logging, and scheduling
  Services/           # Docker detection abstraction and container log collector
  Models/             # Data contracts for detected services
  Dockerfile          # Multi-stage build for the worker container
  appsettings.json    # Default runtime configuration

docker/               # Compose stack definition and auxiliary scripts
  compose.yaml
  automatic1111_patch.py

docs/                 # Operational and configuration references
  reference/
  operations/
  setup/
```

## Support and Contribution
Operational issues and feature proposals should be tracked via the repository issue tracker. When contributing code:
1. Fork the repository and create a feature branch.
2. Adhere to .NET coding conventions and ensure new services include Docker labels for detection.
3. Provide documentation updates (runbook or configuration reference) alongside feature changes.
4. Open a pull request with context, testing evidence, and rollback considerations.

## Revision Flags
- ⚠️ **Automated testing** – No unit or integration tests exist. Add coverage or revise the workflow guidance when tests are available.
- ⚠️ **GPU policy automation** – Resource limits are advisory only. Refresh the GPU policy docs after enforcement logic ships.
- ?? **Log analytics** - Logs now persist under  `/logs`, but no indexing or alerting layer consumes them yet. Revisit once a Loki/Elastic/Promtail integration is prioritised. 
