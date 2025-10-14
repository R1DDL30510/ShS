# Validation Scenarios

This document outlines common validation paths and expected outcomes for the SecureHomeSystem stack. Use these scenarios to verify correct operation of your deployment.

## Core Infrastructure Validation

### 1. Docker Environment
```powershell
# Verify Docker Desktop is running with WSL2 backend
docker info | Select-String "Operating System","OSType","ServerVersion"

# Expected Output:
# Operating System: Windows 10 Pro Version 2009
# OSType: linux
# ServerVersion: 24.0.7
```

### 2. GPU Access
```bash
# In WSL terminal
nvidia-smi

# Verify:
# - Driver version >= 545.23.08
# - CUDA version >= 12.3
# - Available memory meets requirements
```

## Component Validation

### 1. Worker Service

#### Basic Health
```powershell
# Check worker container
docker compose ps shs-worker
# STATUS should be "Up" with "(healthy)"

# Test health endpoint
curl http://localhost:5080/health
# Should return {"status":"Healthy"}

# Verify log files exist
ls data/logs/worker/worker-*.json
# Should see current date's log file
```

#### Service Detection
```powershell
# Watch worker logs
docker compose logs -f shs-worker

# Expected log patterns:
# - Service initialization with URLs
# - Regular 30-second detection cycles
# - Detected services with status
```

### 2. OpenWebUI

#### Connectivity
```powershell
# Verify Ollama connection
curl http://localhost:3000/api/system/info
# Should show "ollama_url": "http://host.docker.internal:11434"

# Test model listing
curl http://localhost:11434/api/tags
# Should list available models
```

#### Authentication
```powershell
# With auth enabled
curl -I http://localhost:3000/api/auth/session
# Should return 401 if not authenticated
```

### 3. Qdrant

#### Database Health
```powershell
# Check readiness
curl http://localhost:6334/readyz
# Should return {"title":"Qdrant is ready"}

# Verify persistence
ls data/qdrant/collections/
# Should see collection directories if RAG is active
```

### 4. Stable Diffusion (Optional)

#### API Functionality
```powershell
# Test minimal generation
$body = @{
    prompt = "test"
    steps = 1
    width = 64
    height = 64
} | ConvertTo-Json

curl -X POST http://localhost:7860/sdapi/v1/txt2img `
     -H "Content-Type: application/json" `
     -d $body
# Should return base64 image data
```

#### Resource Usage
```bash
# Monitor GPU during generation
watch -n 1 nvidia-smi

# Expected patterns:
# - VRAM usage within 80% limit
# - Utilization spikes during generation
```

## Log Collection Validation

### 1. Cursor Management
```powershell
# Check cursor files
ls data/logs/state/*.cursor
# Should see one per service

# Verify content
Get-Content data/logs/state/open-webui.cursor
# Should contain valid JSON with timestamp
```

### 2. Log Rotation
```powershell
# Check service logs
ls data/logs/services/
# Should see active .log files

# After running for >7 days or >10MB
ls data/logs/services/*.log.*
# Should see numbered archives
```

## Common Validation Issues

1. Worker Health Failing
   - Check Docker socket mount
   - Verify process permissions
   - Review worker logs for errors

2. Service Detection Issues
   - Confirm `shs.role` labels in compose.yaml
   - Check container names match expected
   - Verify service is actually running

3. GPU Problems
   - Update NVIDIA drivers
   - Check NVIDIA Container Toolkit
   - Verify VRAM limits in configuration

4. Log Collection Stops
   - Check cursor file permissions
   - Verify log directory exists
   - Review worker logs for IO errors