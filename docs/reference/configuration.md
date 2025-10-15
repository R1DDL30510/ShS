# Configuration Reference

## Table of Contents
- [Environment Variables](#environment-variables)
- [Core Configuration](#core-configuration)
- [Docker Labels Reference](#docker-labels)
- [Health Check Reference](#health-check-reference)
- [Log Format Reference](#log-format-reference)
- [API Response Formats](#api-response-formats)

## Environment Variables

### Core Settings
```env
# Runtime Environment
DOTNET_ENVIRONMENT=Development    # Controls configuration selection
LOG_DIR=../data/logs             # Log storage location
DATA_DIR=../data                 # Application data root

# Network Ports
WORKER_HEALTH_PORT=5080          # Worker health endpoint
WEBUI_PORT=3000                  # OpenWebUI interface
QDRANT_PORT=6334                # Vector database API
AUTOMATIC1111_PORT=7860         # Stable Diffusion UI

# Integration Points
OLLAMA_BASE_URL=http://host.docker.internal:11434  # Ollama endpoint
OLLAMA_MAX_GPU_MEMORY=0.8        # VRAM allocation (0.0-1.0)

# GPU Configuration
AUTOMATIC1111_GPU_SELECTION=all  # GPU device selection
AUTOMATIC1111_GPU_COUNT=all      # Number of GPUs to use
AUTOMATIC1111_ARGS=--medvram --opt-sdp-attention  # SD launch flags
OPENWEBUI_AUTH=true              # Require login for OpenWebUI (set false only for trusted demos)
```

## Core Configuration

The worker loads configuration from `SecureHomeSystem/appsettings.json`, with overrides applied by environment variables or additional JSON files such as `appsettings.Development.json`. The sections below list the supported keys and their defaults in this repository.

## `Docker`
- `ComposeFilePath` (`string`): Path to the Compose file (required, non-empty). Default: `docker/compose.yaml`.
- `EnvironmentFile` (`string`): Relative path to the Compose `.env` file (required, non-empty). Default: `docker/.env`.
- `ProjectName` (`string`): Compose project name used when running CLI commands (required, non-empty). Default: `shs-stack`.
- `AutoStartProfiles` (`string[]`): Compose profiles the worker will attempt to start; empty strings are rejected (repository default: `["worker"]`, development override adds `"diffusion"`).
- `Detection` (`ServiceDetectionOptions`): Nested settings that control how Docker containers are matched.

### `Docker:Detection`
- `LabelSelector` (`Dictionary<string,string>`): Expected Docker label filters keyed by logical service name (must contain at least one mapping with non-empty keys and values). Defaults map the worker to the `shs.role` labels applied in `docker/compose.yaml` (`open-webui`, `qdrant`, `stable-diffusion`).
- `StartupTimeoutSeconds` (`int`): Time window (seconds) allowed for a service to appear before detection reports a timeout (must be ≥ 1). Default: `120`.
- `RetryCount` (`int`): Number of detection retries before escalating (must be ≥ 0). Default: `3`.

## `Services`
### `Services:Ollama`
- `BaseUrl` (`string`): Endpoint used for worker log messages (absolute HTTP/HTTPS URL required). Repository default: `http://host.docker.internal:11434`.
- `MaxGpuMemoryFraction` (`double`): Desired VRAM cap exposed to operators and downstream tooling (range 0.0–1.0). Default: `0.8` (80%).
- `HealthEndpoint` (`string`): Path used for health checks (must start with `/`). Default: `/api/tags`.

### `Services:OpenWebUi`
- `BaseUrl` (`string`): URL announced by the worker (absolute HTTP/HTTPS URL required). Repository default: `http://localhost:3000` (matching the Compose port binding).
- `RequireAuth` (`bool`): Mirrors the authentication flag supplied to the OpenWebUI container. Default: `true`.
- `HealthEndpoint` (`string`): Path used for manual health checks (must start with `/`). Default: `/api/system/info`.

### `Services:StableDiffusion`
- `BaseUrl` (`string`): Stable Diffusion UI endpoint (absolute HTTP/HTTPS URL required). Repository default: `http://localhost:7860`.
- `LaunchProfile` (`string`): Compose profile that must be enabled for Stable Diffusion (required, non-empty). Default: `diffusion`.
- `SmokeTestPrompt` (`string`): Short prompt placeholder for future smoke tests (required, non-empty). Default: `Generate a 64x64 diagnostic image`.

## `ResourceScheduler`
- `GpuUtilisationThreshold` (`double`): Preferred utilisation ceiling recorded in configuration (range 0.0–1.0). Default: `0.5` (50%).
- `GpuMemoryThreshold` (`double`): Preferred VRAM ceiling recorded in configuration (range 0.0–1.0). Default: `0.8` (80%).
- `PollIntervalSeconds` (`int`): Sampling cadence placeholder for future GPU telemetry (must be ≥ 1). Default: `5` (development override reduces to `3`).
- `QueueBackoffSeconds` (`int`): Delay placeholder for retrying queued jobs (must be ≥ 1). Default: `30` (development override reduces to `10`).

> **Note:** The worker currently records resource policy targets for operational awareness; automated enforcement is not yet implemented.

## `Health`
- `Port` (`int`): TCP port bound by the worker for health and liveness endpoints (range 1–65535). Default: `5080`.
- `HealthPath` (`string`): Path exposed via ASP.NET Core health checks, consumed by the Compose health probe (required, non-empty; leading slash enforced automatically). Default: `/health`.
- `LivenessPath` (`string`): Minimal JSON endpoint for quick manual checks (required, non-empty; leading slash enforced automatically). Default: `/live`.

## `Serilog`
- `Using` (`string[]`): Assemblies that expose configured sinks. Defaults include `Serilog.Sinks.Console` and `Serilog.Sinks.File`.
- `MinimumLevel` (`object`): Baseline log level plus overrides for namespaces. Default logs Microsoft categories at `Warning` while keeping the app at `Information`.
- `WriteTo[0]` (`Console`): Streams logs to standard output for Docker logs integration.
- `WriteTo[1]` (`File`): Persists JSON-formatted worker logs to `/logs/worker/worker-.json`. Rotation policy keeps 14 files, rolls daily or at 10 MB.
- `Properties:Application` (`string`): Static property stamped on every log event. Default: `SecureHomeSystem.Worker`.

## `LogStorage`
- `RootPath` (`string`): Base directory for all persisted logs and cursors (required, non-empty). Default: `/logs` (mapped from `${LOG_DIR}` in Compose).
- `WorkerFolderName` (`string`): Subdirectory for Serilog output (required, non-empty). Default: `worker`.
- `ServicesFolderName` (`string`): Subdirectory for harvested container logs (required, non-empty). Default: `services`.
- `StateFolderName` (`string`): Subdirectory for per-service cursor files (required, non-empty). Default: `state`.

## `LogCollector`
- `Enabled` (`bool`): Toggle for the docker log harvester. Default: `true`.
- `PollIntervalSeconds` (`int`): Delay between collection passes (must be ≥ 1 second). Default: `30`.
- `InitialLookbackMinutes` (`int`): How far back to fetch logs on the first run when no cursor exists (must be ≥ 1 minute). Default: `10`.
- `Rotation` (`LogRotationOptions`): Retention policy applied to `/logs/services/<service>.log`.
  - `MaxFileSizeBytes` (`long`): Rotate when the active log reaches this size (0 disables size-based rotation; negative values are rejected). Default: `10485760` (10 MB).
  - `MaxFileAgeDays` (`int`): Rotate when the active log is at least this many days old (0 disables age-based rotation; negative values are rejected). Default: `7`.
  - `MaxArchiveFiles` (`int`): Maximum number of rotated archives to retain per service (0 disables pruning by count; negative values are rejected). Default: `5`.

## Docker Labels Reference

### Service Identification
| Label | Purpose | Example |
|-------|----------|---------|
| `shs.role` | Service type identifier | `shs.role=open-webui` |
| `shs.version` | Component version | `shs.version=v0.3.7` |
| `shs.profile` | Required profile | `shs.profile=diffusion` |

### Health Monitoring
| Label | Purpose | Example |
|-------|----------|---------|
| `shs.health.port` | Health check port | `shs.health.port=8080` |
| `shs.health.path` | Health endpoint | `shs.health.path=/health` |
| `shs.health.interval` | Check frequency | `shs.health.interval=30s` |

### Resource Management
| Label | Purpose | Example |
|-------|----------|---------|
| `shs.gpu.required` | GPU requirement flag | `shs.gpu.required=true` |
| `shs.gpu.memory` | VRAM limit | `shs.gpu.memory=0.8` |
| `shs.gpu.count` | GPU count | `shs.gpu.count=1` |

## Health Check Reference

### Worker Health Check
```http
GET http://localhost:5080/health
```
```json
{
  "status": "Healthy",
  "results": {
    "self": {
      "status": "Healthy",
      "description": null,
      "data": {}
    }
  }
}
```

### OpenWebUI Health Check
```http
GET http://localhost:3000/api/system/info
```
```json
{
  "version": "0.3.7",
  "ollama_url": "http://host.docker.internal:11434",
  "persistent_config": false,
  "auth_enabled": true
}
```

### Qdrant Health Check
```http
GET http://localhost:6334/readyz
```
```json
{
  "title": "Qdrant is ready",
  "status": "ok",
  "timestamp": "2025-10-14T10:00:00.789Z"
}
```

### Stable Diffusion Health Check
```http
POST http://localhost:7860/sdapi/v1/txt2img
Content-Type: application/json

{
  "prompt": "test",
  "steps": 1,
  "width": 64,
  "height": 64
}
```
```json
{
  "images": ["<base64-image>"],
  "parameters": {
    "prompt": "test",
    "steps": 1,
    "width": 64,
    "height": 64
  },
  "info": "<generation-info>"
}
```

## Log Format Reference

### Worker JSON Log
```json
{
  "Timestamp": "2025-10-14T10:00:00.123Z",
  "Level": "Information",
  "MessageTemplate": "string",
  "Properties": {
    "Application": "SecureHomeSystem.Worker",
    "Environment": "Development",
    "ServiceName": "string",
    "ContainerId": "string",
    "Status": "string"
  },
  "Exception": "string?"
}
```

### Service Log Format
```
<timestamp> [<level>] <message>
2025-10-14T10:00:00.123Z [INFO] Starting service
```

### Cursor File Format
```json
{
  "LastProcessedTimestamp": "2025-10-14T10:00:00.123Z",
  "ContainerId": "string",
  "ServiceName": "string"
}
```

## API Response Formats

### Service Detection Response
```json
{
  "services": [
    {
      "name": "string",
      "containerId": "string",
      "image": "string",
      "status": "string",
      "isRunning": true,
      "labels": {
        "shs.role": "string"
      }
    }
  ]
}
```

### Resource Metrics Format
```json
{
  "timestamp": "2025-10-14T10:00:00.123Z",
  "gpu": {
    "utilization": 0.42,
    "memoryUsed": 11024,
    "memoryTotal": 24576
  },
  "container": {
    "id": "string",
    "name": "string",
    "gpuDevices": ["0"],
    "memoryUsage": 8192
  }
}
```

### Error Response Format
```json
{
  "error": {
    "code": "string",
    "message": "string",
    "details": [
      {
        "type": "string",
        "message": "string"
      }
    ]
  },
  "correlationId": "string"
}
```
