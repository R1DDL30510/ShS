# GPU Policy

The repository captures desired GPU utilisation targets in configuration and Compose settings. Automation for dynamic scheduling has not yet been implemented, so operators enforce these limits manually.

## Configuration Targets
- `ResourceScheduler:GpuUtilisationThreshold = 0.5`: preferred ceiling for sustained GPU utilisation.
- `ResourceScheduler:GpuMemoryThreshold = 0.8`: desired VRAM cap per device.
- `ResourceScheduler:PollIntervalSeconds = 5` / `QueueBackoffSeconds = 30`: placeholders that document the intended sampling and retry cadence (development overrides reduce these to `3` and `10`).

## Container Parameters
- Ollama: `OLLAMA_MAX_GPU_MEMORY` defaults to `0.8` in `docker/compose.yaml`, aligning with the VRAM target.
- Stable Diffusion: the Compose command line includes `--medvram` and `--opt-sdp-attention` to control memory usage; additional arguments can be supplied via the `AUTOMATIC1111_ARGS` environment variable.
- GPU access is enabled for Stable Diffusion through the `diffusion` profile (`deploy.resources.reservations.devices`).

## Operational Guidance
- Monitor real-time utilisation with `nvidia-smi` (WSL2 or host shell) or vendor dashboards.
- Pause heavy jobs by stopping the Stable Diffusion container: `docker compose --profile diffusion stop automatic1111`.
- Resume workloads once utilisation falls below the configured targets and restart containers with `docker compose up -d`.

## Future Enhancements
- Implement automated enforcement based on the recorded thresholds.
- Surface telemetry and alerting (metrics/events) once scheduling logic is in place.
