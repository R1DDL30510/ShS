# Configuration Reference

## `Docker`
- `ComposeFilePath` (`string`): Path to `docker/compose.yaml`.
- `EnvironmentFile` (`string`): Path to the `.env` file used by Compose.
- `ProjectName` (`string`): Compose project name (default `shs-stack`).
- `AutoStartProfiles` (`string[]`): Profiles automatically started by the worker (e.g. `["worker"]`).

## `Ollama`
- `BaseUrl` (`string`): Default `http://host.docker.internal:11434`.
- `MaxGpuMemoryFraction` (`double`): Fractional VRAM cap (0.8 = 80%).
- `HealthEndpoint` (`string`): Path used for availability checks (`GET /api/tags`).

## `OpenWebUI`
- `BaseUrl` (`string`): `http://localhost:${WEBUI_PORT}` (default `3003`).
- `RequireAuth` (`bool`): Mirrors the `WEBUI_AUTH` env flag.
- `HealthEndpoint` (`string`): `GET /api/system/info`.

## `StableDiffusion`
- `BaseUrl` (`string`): `http://localhost:${AUTOMATIC1111_PORT}`.
- `LaunchProfile` (`string`): Compose profile containing the service (default `diffusion`).
- `SmokeTestPrompt` (`string`): Short prompt used for health tests.

## `ResourceScheduler`
- `GpuUtilisationThreshold` (`double`): 0.5 means queue heavy work once utilisation > 50%.
- `GpuMemoryThreshold` (`double`): 0.8 means pause new jobs beyond 80% VRAM.
- `PollIntervalSeconds` (`int`): Sampling cadence for NVML / `nvidia-smi`.
- `QueueBackoffSeconds` (`int`): Delay before the scheduler retries queued work.

## `ServiceDetection`
- `LabelSelector` (`Dictionary<string,string>`): Expected Docker labels (key = service, value = label match).
- `StartupTimeoutSeconds` (`int`): Time window before detection fails a service.
- `RetryCount` (`int`): Restart attempts before the worker escalates.
- The worker expects the Docker CLI (`docker`) to be available. In container scenarios the compose file mounts `/var/run/docker.sock`.
