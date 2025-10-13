# GPU Policy

## Targets
- Cap VRAM usage at ~80% per GPU.
- Queue new heavy jobs whenever compute utilisation exceeds 50%.
- Distribute workloads across both 12 GB GPUs.

## Scheduler Mechanics
- Poll GPU metrics via NVML (`GpuMetricsCollector`) every 5 seconds.
- `GpuScheduler.AcquireAsync()` picks the least loaded GPU below the utilisation and memory thresholds; otherwise it queues the job.
- Jobs release GPU slots on completion or timeout.

## Service Limits
- Ollama: `OLLAMA_MAX_GPU_MEMORY=0.8` plus model-specific limits.
- Stable Diffusion: `--medvram`, `--opt-sdp-attention`, optional `--precision full` if required.
- Additional services can override thresholds through configuration.

## Escalations
- If queue length > 5 or wait time > 2 minutes, emit warnings (logs + events).
- Operators can intervene with `docker compose stop automatic1111` or cancel queued jobs through the API.

## Tests
- Smoke tests simulate concurrent prompts to verify queue behaviour and the 80% guardrail.
- Planned observability: expose Prometheus metrics for GPU utilisation and queue length.
