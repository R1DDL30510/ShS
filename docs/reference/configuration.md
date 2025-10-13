# Configuration Reference

This document captures the authoritative configuration knobs for the SHS worker and the Docker stack. Environment variables can be supplied through `docker/.env`, `appsettings.json`, or the Windows service configuration.

## Docker Environment
- `DOTNET_ENVIRONMENT`: passes the hosting environment into `shs-worker`.
- `WEBUI_PORT`: host port mapped to Open WebUI (`default: 3000`).
- `QDRANT_PORT`: host port mapped to Qdrant (`default: 6333`).
- `AUTOMATIC1111_PORT`: host port mapped to Automatic1111 (`default: 7860`).
- `DATA_DIR`: root directory for Open WebUI and Qdrant data (`default: ../data`).
- `AUTOMATIC1111_DATA_DIR`: persistent storage for Automatic1111 outputs (`default: ../data/automatic1111`).
- `AUTOMATIC1111_MODELS_DIR`: checkpoint directory (`default: ../models/stable-diffusion`).
- `AUTOMATIC1111_WORKSPACE`: bind-mount of the web UI workspace (`default: ../stablediff/stable-diffusion-webui`).
- `AUTOMATIC1111_ARGS`: command-line arguments passed to Automatic1111. Defaults enforce API access, 80% GPU memory cap (`--medvram`), and attention optimisations.
- `AUTOMATIC1111_GPU_SELECTION`: GPU IDs exposed inside the container (`default: all`).
- `AUTOMATIC1111_GPU_COUNT`: number of GPUs reserved via compose deploy stanza (`default: all`).
- `OLLAMA_BASE_URL`: endpoint used by Open WebUI to reach the Windows-hosted Ollama service (`default: http://host.docker.internal:11434`).
- `OLLAMA_MAX_GPU_MEMORY`: fraction of GPU memory Ollama should use; the worker also enforces the 80% ceiling.
- `OPENWEBUI_AUTH`: toggles authentication in Open WebUI (`default: false`).

## Worker Settings (appsettings.json)
- `Docker:ComposeFile`: relative path to the compose file (`default: docker/compose.yaml`).
- `Docker:ProjectName`: compose project name (`default: shs-stack`).
- `Docker:Profiles`: list of profiles to activate (`default: ["worker"]`).
- `Services:OpenWebUI:Url`: base URL to connect to (discovered automatically when possible).
- `Services:StableDiffusion:Url`: base URL for Automatic1111.
- `Services:Ollama:Url`: defaults to `http://localhost:11434` when running on Windows.
- `ResourceScheduler:GpuUtilisationThreshold`: utilisation percentage that triggers queueing (`default: 50`).
- `ResourceScheduler:MemoryHeadroom`: percentage of GPU memory to leave unused (`default: 20`).
- `ResourceScheduler:PollingInterval`: interval between GPU usage polls (`default: 00:00:10`).

Refer to `docs/operations/runbook.md` for usage guidance and to `appsettings.json` for concrete defaults.
