# SecureHomeSystem Stack Examples and Validation

This document provides example outputs, validation scenarios, and expected behaviors for each component of the SecureHomeSystem stack. Use these examples to validate your deployment and understand normal operation patterns.

## Worker Service Examples

### Normal Operation Logs
```json
{
  "Timestamp": "2025-10-14T10:00:00.123Z",
  "Level": "Information",
  "MessageTemplate": "SecureHomeSystem worker initialised. Ollama: {OllamaUrl}, OpenWebUI: {OpenWebUiUrl}, StableDiffusion: {StableDiffusionUrl}",
  "Properties": {
    "OllamaUrl": "http://host.docker.internal:11434",
    "OpenWebUiUrl": "http://localhost:3000",
    "StableDiffusionUrl": "http://localhost:7860"
  }
}
```

### Service Detection Output
```json
{
  "Timestamp": "2025-10-14T10:00:30.456Z",
  "Level": "Information",
  "MessageTemplate": "Detected service {ServiceName} | Container {ContainerId} | Image {Image} | Status {Status} | Running {IsRunning}",
  "Properties": {
    "ServiceName": "open-webui",
    "ContainerId": "abc123def456",
    "Image": "ghcr.io/open-webui/open-webui:v0.3.7",
    "Status": "running",
    "IsRunning": true
  }
}
```

### Health Check Response
```json
// GET http://localhost:5080/health
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

## OpenWebUI Examples

### System Info Response
```json
// GET http://localhost:3000/api/system/info
{
  "version": "0.3.7",
  "ollama_url": "http://host.docker.internal:11434",
  "persistent_config": false,
  "auth_enabled": true
}
```

### Expected Directory Structure
```
data/open-webui/
├── config.json           # Runtime configuration
├── cache/               # Embeddings and generations
│   ├── embedding/
│   └── image/
└── vector_db/          # RAG database files
```

## Qdrant Integration

### Health Check Response
```json
// GET http://localhost:6334/readyz
{
  "title": "Qdrant is ready",
  "status": "ok",
  "timestamp": "2025-10-14T10:00:00.789Z"
}
```

### Expected Directory Structure
```
data/qdrant/
├── .qdrant_fs_check
├── raft_state.json
├── aliases/
│   └── data.json
└── collections/
```

## Stable Diffusion Examples

### API Test Response
```json
// POST http://localhost:7860/sdapi/v1/txt2img
// Request: {"prompt": "test", "steps": 1, "width": 64, "height": 64}
{
  "images": ["<base64-encoded-image>"],
  "parameters": {
    "prompt": "test",
    "steps": 1,
    "width": 64,
    "height": 64
  },
  "info": "<generation-info-string>"
}
```

### Expected Directory Structure
```
data/automatic1111/
├── config.json          # UI configuration
├── params.txt          # Generation parameters
├── ui-config.json     # Interface settings
└── Models/           # Model directories
    ├── Stable-diffusion/
    ├── ESRGAN/
    ├── GFPGAN/
    └── Lora/
```

### GPU Resource Usage
```bash
# Example nvidia-smi output
+-----------------------------------------------------------------------------+
| NVIDIA-SMI 545.23.08    Driver Version: 545.23.08    CUDA Version: 12.3     |
|-------------------------------+----------------------+----------------------+
| GPU  Name        Persistence-M| Bus-Id        Disp.A | Volatile Uncorr. ECC |
| Fan  Temp  Perf  Pwr:Usage/Cap|         Memory-Usage | GPU-Util  Compute M. |
|===============================+======================+======================|
|   0  NVIDIA RTX 4090    Off  | 00000000:01:00.0  On|                  N/A |
| 30%   45C    P2    71W / 450W|  11024MiB / 24576MiB|     42%      Default |
+-------------------------------+----------------------+----------------------+
```

## Log Collection Examples

### Container Log Format
```
data/logs/services/open-webui.log:
2025-10-14T10:00:00.123Z [INFO] Starting OpenWebUI v0.3.7
2025-10-14T10:00:01.456Z [INFO] Connected to Ollama at http://host.docker.internal:11434
2025-10-14T10:00:02.789Z [INFO] Server listening on :8080
```

### Log Cursor State
```json
// data/logs/state/open-webui.cursor
{
  "LastProcessedTimestamp": "2025-10-14T10:00:02.789Z",
  "ContainerId": "abc123def456",
  "ServiceName": "open-webui"
}
```

## Validation Tips

1. Always check worker logs first (`docker compose logs shs-worker`)
2. Verify GPU access with `nvidia-smi` in WSL
3. Test health endpoints in sequence (worker -> services)
4. Examine log files under `data/logs/` for issues
5. Check cursor files if log collection stops