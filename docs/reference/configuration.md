# Configuration Reference

The worker loads configuration from `SecureHomeSystem/appsettings.json`, with overrides applied by environment variables or additional JSON files such as `appsettings.Development.json`. The sections below list the supported keys and their defaults in this repository.

## `Docker`
- `ComposeFilePath` (`string`): Path to the Compose file. Default: `docker/compose.yaml`.
- `EnvironmentFile` (`string`): Relative path to the Compose `.env` file. Default: `docker/.env`.
- `ProjectName` (`string`): Compose project name used when running CLI commands. Default: `shs-stack`.
- `AutoStartProfiles` (`string[]`): Compose profiles the worker will attempt to start (repository default: `["worker"]`, development override adds `"diffusion"`).
- `Detection` (`ServiceDetectionOptions`): Nested settings that control how Docker containers are matched.

### `Docker:Detection`
- `LabelSelector` (`Dictionary<string,string>`): Expected Docker label filters keyed by logical service name. Defaults map the worker to the `shs.role` labels applied in `docker/compose.yaml` (`open-webui`, `qdrant`, `stable-diffusion`).
- `StartupTimeoutSeconds` (`int`): Time window (seconds) allowed for a service to appear before detection reports a timeout. Default: `120`.
- `RetryCount` (`int`): Number of detection retries before escalating. Default: `3`.

## `Services`
### `Services:Ollama`
- `BaseUrl` (`string`): Endpoint used for worker log messages. Repository default: `http://host.docker.internal:11434`.
- `MaxGpuMemoryFraction` (`double`): Desired VRAM cap exposed to operators and downstream tooling. Default: `0.8` (80%).
- `HealthEndpoint` (`string`): Path used for health checks. Default: `/api/tags`.

### `Services:OpenWebUi`
- `BaseUrl` (`string`): URL announced by the worker. Repository default: `http://localhost:3003` (matching the Compose port binding).
- `RequireAuth` (`bool`): Mirrors the authentication flag supplied to the OpenWebUI container. Default: `false`.
- `HealthEndpoint` (`string`): Path used for manual health checks. Default: `/api/system/info`.

### `Services:StableDiffusion`
- `BaseUrl` (`string`): Stable Diffusion UI endpoint. Repository default: `http://localhost:7860`.
- `LaunchProfile` (`string`): Compose profile that must be enabled for Stable Diffusion. Default: `diffusion`.
- `SmokeTestPrompt` (`string`): Short prompt placeholder for future smoke tests. Default: `Generate a 64x64 diagnostic image`.

## `ResourceScheduler`
- `GpuUtilisationThreshold` (`double`): Preferred utilisation ceiling recorded in configuration. Default: `0.5` (50%).
- `GpuMemoryThreshold` (`double`): Preferred VRAM ceiling recorded in configuration. Default: `0.8` (80%).
- `PollIntervalSeconds` (`int`): Sampling cadence placeholder for future GPU telemetry. Default: `5` (development override reduces to `3`).
- `QueueBackoffSeconds` (`int`): Delay placeholder for retrying queued jobs. Default: `30` (development override reduces to `10`).

> **Note:** The worker currently records resource policy targets for operational awareness; automated enforcement is not yet implemented.
