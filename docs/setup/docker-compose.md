# Docker Compose Setup

This repository ships with a Compose stack that bundles the SHS worker, Open WebUI, Qdrant, and the optional Automatic1111 Stable Diffusion runtime.

## Prerequisites
- Docker Desktop 4.30+ with the NVIDIA container toolkit enabled.
- NVIDIA drivers capable of exposing both GPUs to containers.
- A copy of `docker/.env.example` duplicated as `docker/.env` with values adjusted to your environment.

## Usage
1. Copy the environment template:
   ```sh
   cd docker
   cp .env.example .env
   ```
2. Review ports, data directories, and GPU selections in `.env`. Paths default to `../data` and `../models` relative to the repository root.
3. Launch the stack:
   ```sh
   docker compose -f docker/compose.yaml --profile worker up -d
   ```
4. Enable Stable Diffusion workloads by adding the `diffusion` profile:
   ```sh
   docker compose -f docker/compose.yaml --profile diffusion up -d
   ```
5. Stop services when finished:
   ```sh
   docker compose -f docker/compose.yaml down
   ```

## Service Notes
- Ollama runs as a native Windows service and is not part of the Compose stack. `OLLAMA_BASE_URL` defaults to `http://host.docker.internal:11434`.
- Open WebUI and Qdrant share the `DATA_DIR` volume; Automatic1111 uses dedicated volumes for checkpoints and generated assets.
- Labels such as `shs.role=open-webui` are attached to each container and are consumed by the worker to detect running services.

Refer to `docs/reference/configuration.md` for the full set of environment variables and to `docs/operations/runbook.md` for operational procedures.
