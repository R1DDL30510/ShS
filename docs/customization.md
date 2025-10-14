# Stack Customization Guide

This guide explains how to customize and configure the SecureHomeSystem stack for your specific needs. It covers environment variables, configuration options, model selection, and common modifications.

## Environment Configuration

### Base Environment Variables
Create a `docker/.env` file based on `docker/.env.example`:

```env
# Core Settings
DOTNET_ENVIRONMENT=Development
LOG_DIR=../data/logs
DATA_DIR=../data

# Port Configuration
WORKER_HEALTH_PORT=5080
WEBUI_PORT=3000
QDRANT_PORT=6334
AUTOMATIC1111_PORT=7860

# Ollama Integration
OLLAMA_BASE_URL=http://host.docker.internal:11434
OLLAMA_MAX_GPU_MEMORY=0.8

# GPU Settings
AUTOMATIC1111_GPU_SELECTION=all
AUTOMATIC1111_GPU_COUNT=all
```

### Worker Configuration
Modify `SecureHomeSystem/appsettings.json` for worker behavior:

```json
{
  "Docker": {
    "ComposeFilePath": "docker/compose.yaml",
    "EnvironmentFile": "docker/.env",
    "ProjectName": "shs-stack",
    "AutoStartProfiles": ["worker"],
    "Detection": {
      "LabelSelector": {
        "open-webui": "shs.role=open-webui",
        "qdrant": "shs.role=qdrant",
        "stable-diffusion": "shs.role=stable-diffusion"
      },
      "StartupTimeoutSeconds": 120,
      "RetryCount": 3
    }
  },
  "ResourceScheduler": {
    "GpuUtilisationThreshold": 0.5,
    "GpuMemoryThreshold": 0.8,
    "PollIntervalSeconds": 5,
    "QueueBackoffSeconds": 30
  }
}
```

## Component Customization

### 1. OpenWebUI Configuration

#### Authentication and UI
```yaml
# In docker/compose.yaml
open-webui:
  environment:
    - WEBUI_NAME=SecureHomeGPT  # Custom UI title
    - ENABLE_PERSISTENT_CONFIG=False
    - WEBUI_AUTH=true  # Enable authentication
```

#### Vector Database Settings
```json
// data/open-webui/config.json
{
  "embedding_model": "all-MiniLM-L6-v2",
  "vector_db": {
    "implementation": "qdrant",
    "path": "/app/backend/data/vector_db"
  }
}
```

### 2. Stable Diffusion Setup

#### Model Management
```bash
# Place models in specific directories:
models/stable-diffusion/
├── deliberate_v2.safetensors     # Main SD model
├── v1-5-pruned-emaonly.safetensors  # Base model
└── custom-model.safetensors      # Your custom model

# Update AUTOMATIC1111_ARGS in compose.yaml:
AUTOMATIC1111_ARGS=--listen --port 7860 --api --xformers --data-dir /data --ckpt-dir /data/Models --medvram --opt-sdp-attention
```

#### GPU Optimization
```yaml
# In docker/compose.yaml
automatic1111:
  environment:
    - NVIDIA_VISIBLE_DEVICES=${AUTOMATIC1111_GPU_SELECTION:-all}
    - NVIDIA_DRIVER_CAPABILITIES=compute,utility
  deploy:
    resources:
      reservations:
        devices:
          - driver: nvidia
            count: ${AUTOMATIC1111_GPU_COUNT:-all}
            capabilities: [gpu]
```

### 3. Log Collection Customization

#### Rotation Policy
```json
// In appsettings.json
{
  "LogCollector": {
    "Enabled": true,
    "PollIntervalSeconds": 30,
    "InitialLookbackMinutes": 10,
    "Rotation": {
      "MaxFileSizeBytes": 10485760,  // 10MB
      "MaxFileAgeDays": 7,
      "MaxArchiveFiles": 5
    }
  }
}
```

#### Storage Paths
```json
{
  "LogStorage": {
    "RootPath": "/logs",
    "WorkerFolderName": "worker",
    "ServicesFolderName": "services",
    "StateFolderName": "state"
  }
}
```

## Development Customizations

### 1. Adding New Services

1. Add service definition to `docker/compose.yaml`:
```yaml
new-service:
  image: your-image:tag
  restart: unless-stopped
  ports:
    - "${NEW_SERVICE_PORT:-8080}:8080"
  volumes:
    - ${DATA_DIR:-../data}/new-service:/data
  labels:
    shs.role: new-service
```

2. Add detection configuration:
```json
// In appsettings.json
{
  "Docker": {
    "Detection": {
      "LabelSelector": {
        "new-service": "shs.role=new-service"
      }
    }
  }
}
```

3. Create service options class:
```csharp
// In Configuration/NewServiceOptions.cs
public class NewServiceOptions
{
    public string BaseUrl { get; set; } = "http://localhost:8080";
    public string HealthEndpoint { get; set; } = "/health";
}
```

### 2. Custom Health Checks

Add new health checks to `Program.cs`:
```csharp
builder.Services
    .AddHealthChecks()
    .AddCheck("new-service", () => 
    {
        // Custom health logic
        return HealthCheckResult.Healthy();
    });
```

### 3. Development Environment

Create `appsettings.Development.json` for local settings:
```json
{
  "ResourceScheduler": {
    "PollIntervalSeconds": 3,
    "QueueBackoffSeconds": 10
  },
  "Docker": {
    "AutoStartProfiles": ["worker", "diffusion"]
  }
}
```

## Common Customization Scenarios

### 1. Changing Resource Limits
```yaml
# In docker/compose.yaml
services:
  automatic1111:
    environment:
      - AUTOMATIC1111_ARGS=--medvram --opt-sdp-attention
      - OLLAMA_MAX_GPU_MEMORY=0.6  # Reduce to 60% VRAM
```

### 2. Adding Custom Models
1. Place models in appropriate directories:
   ```
   models/stable-diffusion/   # SD checkpoints
   data/automatic1111/Models/ # Other model types
   ```
2. Update configuration to recognize new models
3. Restart affected services

### 3. Modifying Detection Intervals
```json
// In appsettings.json
{
  "Worker": {
    "DetectionIntervalSeconds": 30
  },
  "LogCollector": {
    "PollIntervalSeconds": 60
  }
}
```

### 4. Customizing Log Formats
```json
// In appsettings.json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "File",
        "Args": {
          "path": "/logs/worker/worker-.json",
          "rollingInterval": "Day",
          "fileSizeLimitBytes": "10485760",
          "retainedFileCountLimit": 14
        }
      }
    ]
  }
}
```

## Quick Reference

### Environment Variables
| Variable | Purpose | Default |
|----------|----------|---------|
| `DOTNET_ENVIRONMENT` | Runtime environment | Development |
| `WORKER_HEALTH_PORT` | Worker health endpoint | 5080 |
| `OLLAMA_MAX_GPU_MEMORY` | VRAM limit for Ollama | 0.8 |
| `AUTOMATIC1111_GPU_SELECTION` | GPU device selection | all |

### Configuration Paths
| Path | Purpose |
|------|----------|
| `docker/.env` | Environment variables |
| `appsettings.json` | Core worker config |
| `data/open-webui/config.json` | OpenWebUI settings |
| `data/automatic1111/config.json` | SD WebUI config |

### Service Labels
| Label | Service |
|-------|----------|
| `shs.role=open-webui` | OpenWebUI |
| `shs.role=qdrant` | Vector database |
| `shs.role=stable-diffusion` | Stable Diffusion |