# Troubleshooting Guide

This guide provides comprehensive troubleshooting steps for common issues in the SecureHomeSystem stack. Each section includes problem identification, diagnostic steps, and solutions.

## Quick Diagnostic Steps

1. Check system status:
```powershell
# Check all containers
docker compose ps

# View recent logs
docker compose logs --tail=100

# Check GPU status
wsl nvidia-smi
```

2. Verify health endpoints:
```powershell
curl http://localhost:5080/health    # Worker
curl http://localhost:3000/api/system/info  # OpenWebUI
curl http://localhost:6334/readyz    # Qdrant
```

## Worker Service Issues

### Worker Won't Start

#### Symptoms
- Container exits immediately
- Health check fails
- No log files generated

#### Diagnosis
```powershell
# Check container logs
docker compose logs shs-worker

# Verify Docker socket mount
docker compose config | Select-String "docker.sock"

# Check log directory permissions
ls -l data/logs/worker/
```

#### Solutions
1. Docker Socket Issues:
   ```yaml
   # Ensure docker/compose.yaml has:
   volumes:
     - /var/run/docker.sock:/var/run/docker.sock
   ```

2. Permission Problems:
   ```bash
   # Fix log directory permissions
   sudo chown -R 1000:1000 data/logs
   ```

3. Configuration Errors:
   ```powershell
   # Verify appsettings.json exists and is valid
   Get-Content SecureHomeSystem/appsettings.json | ConvertFrom-Json
   ```

### Service Detection Fails

#### Symptoms
- No services reported in logs
- "No managed docker services detected" messages
- Services running but not detected

#### Diagnosis
```powershell
# Check container labels
docker compose ps --format json | ConvertFrom-Json | Select-Object Service, Labels

# Verify detection configuration
Get-Content SecureHomeSystem/appsettings.json | Select-String "LabelSelector"
```

#### Solutions
1. Label Mismatch:
   ```yaml
   # Ensure services have proper labels in compose.yaml:
   labels:
     shs.role: open-webui  # Must match LabelSelector
   ```

2. Docker CLI Access:
   ```powershell
   # Test Docker CLI access
   docker ps  # Should work without sudo
   ```

## OpenWebUI Integration

### Connection to Ollama Failed

#### Symptoms
- "Failed to connect to Ollama" in UI
- Model operations fail
- System info shows incorrect Ollama URL

#### Diagnosis
```powershell
# Check Ollama accessibility
curl http://host.docker.internal:11434/api/tags

# Verify OpenWebUI configuration
docker compose exec open-webui env | Select-String "OLLAMA"
```

#### Solutions
1. Ollama Not Running:
   ```powershell
   # Start Ollama service
   ollama serve
   ```

2. Network Issues:
   ```yaml
   # Update docker/compose.yaml
   environment:
     - OLLAMA_BASE_URL=http://host.docker.internal:11434
   ```

### Vector Database Problems

#### Symptoms
- RAG features not working
- Missing chat history
- Embedding errors

#### Diagnosis
```powershell
# Check Qdrant health
curl http://localhost:6334/readyz

# Verify database files
ls data/qdrant/collections/
```

#### Solutions
1. Reset Vector DB:
   ```powershell
   # Stop services
   docker compose down
   
   # Clear vector database
   Remove-Item data/qdrant/collections/* -Recurse
   
   # Restart stack
   docker compose --profile worker up -d
   ```

## Stable Diffusion Issues

### GPU Access Problems

#### Symptoms
- CUDA errors in logs
- Very slow generation
- Out of memory errors

#### Diagnosis
```bash
# Check GPU visibility
docker compose exec automatic1111 nvidia-smi

# View resource limits
docker compose exec automatic1111 env | grep -E "NVIDIA|CUDA"
```

#### Solutions
1. Driver Issues:
   ```bash
   # Update NVIDIA drivers in WSL
   sudo apt update
   sudo apt install nvidia-driver-545
   ```

2. Memory Management:
   ```yaml
   # Modify compose.yaml
   environment:
     - AUTOMATIC1111_ARGS=--medvram --opt-sdp-attention
   ```

3. GPU Selection:
   ```env
   # Update docker/.env
   AUTOMATIC1111_GPU_SELECTION=0  # Use specific GPU
   AUTOMATIC1111_GPU_COUNT=1      # Limit to one GPU
   ```

### Model Loading Failed

#### Symptoms
- "Error loading model" messages
- Blank model list
- Generation fails to start

#### Diagnosis
```powershell
# Check model files
ls models/stable-diffusion/*.safetensors

# Verify model directory mounting
docker compose config | Select-String "Models"
```

#### Solutions
1. Model Path Issues:
   ```yaml
   # Update compose.yaml volumes
   volumes:
     - ${AUTOMATIC1111_MODELS_DIR:-../models/stable-diffusion}:/data/Models
   ```

2. Model Access:
   ```powershell
   # Fix model permissions
   sudo chown -R 1000:1000 models/
   ```

## Log Collection Issues

### Missing Container Logs

#### Symptoms
- No logs in data/logs/services/
- Stale cursor files
- Gaps in log coverage

#### Diagnosis
```powershell
# Check cursor files
Get-Content data/logs/state/*.cursor

# Verify log collector status
docker compose logs shs-worker | Select-String "LogCollector"
```

#### Solutions
1. Reset Cursors:
   ```powershell
   # Remove cursor files
   Remove-Item data/logs/state/*.cursor
   
   # Restart worker
   docker compose restart shs-worker
   ```

2. Permission Issues:
   ```bash
   # Fix log directory ownership
   sudo chown -R 1000:1000 data/logs/
   ```

### Log Rotation Problems

#### Symptoms
- Growing log files
- No rotation occurring
- Disk space warnings

#### Diagnosis
```powershell
# Check log sizes
Get-ChildItem data/logs/services/ -Recurse | Sort-Object Length -Descending | Select-Object Name, Length

# Verify rotation settings
Get-Content SecureHomeSystem/appsettings.json | Select-String "Rotation"
```

#### Solutions
1. Update Rotation Policy:
   ```json
   // In appsettings.json
   "LogCollector": {
     "Rotation": {
       "MaxFileSizeBytes": 10485760,
       "MaxFileAgeDays": 7,
       "MaxArchiveFiles": 5
     }
   }
   ```

2. Manual Rotation:
   ```powershell
   # Archive large logs
   Move-Item data/logs/services/service.log data/logs/services/service.log.1
   ```

## System Resource Issues

### High Memory Usage

#### Symptoms
- Container OOM kills
- System slowdown
- Swap thrashing

#### Solutions
1. Container Limits:
   ```yaml
   # Update compose.yaml
   services:
     automatic1111:
       deploy:
         resources:
           limits:
             memory: 8G
   ```

2. Runtime Options:
   ```yaml
   environment:
     - AUTOMATIC1111_ARGS=--medvram --opt-sdp-attention
     - OLLAMA_MAX_GPU_MEMORY=0.6
   ```

### Disk Space Problems

#### Symptoms
- Container start failures
- Write errors
- No space warnings

#### Solutions
1. Log Cleanup:
   ```powershell
   # Clear old logs
   Get-ChildItem data/logs -Recurse -Filter "*.log.*" | 
     Where-Object { $_.LastWriteTime -lt (Get-Date).AddDays(-7) } |
     Remove-Item
   ```

2. Cache Cleanup:
   ```powershell
   # Clear OpenWebUI cache
   Remove-Item data/open-webui/cache/* -Recurse
   ```

## Common Error Messages

| Error Message | Likely Cause | Solution |
|--------------|--------------|----------|
| "Failed to detect services" | Docker socket permission/access | Check socket mount and permissions |
| "CUDA out of memory" | GPU memory exhausted | Adjust VRAM limits, use --medvram |
| "Could not connect to Ollama" | Ollama not running/accessible | Check Ollama service and URL |
| "Error writing to log file" | Permission or space issues | Check permissions and disk space |
| "Invalid configuration value" | appsettings.json error | Validate JSON syntax and values |

## Preventive Measures

1. Regular Monitoring:
   ```powershell
   # Create monitoring script
   $checks = @(
     "docker compose ps",
     "curl http://localhost:5080/health",
     "wsl nvidia-smi"
   )
   ```

2. Log Management:
   ```powershell
   # Scheduled log cleanup
   Get-ChildItem data/logs -Recurse |
     Where-Object { $_.LastWriteTime -lt (Get-Date).AddDays(-14) } |
     Remove-Item
   ```

3. Resource Monitoring:
   ```bash
   # GPU monitoring script
   watch -n 5 'nvidia-smi --query-gpu=utilization.gpu,memory.used,memory.total --format=csv'
   ```