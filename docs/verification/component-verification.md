# Component Verification Checklists

## Prerequisites Verification

### System Requirements
- [ ] Docker Desktop with WSL2 backend
  ```powershell
  # Check Docker version and backend
  docker version
  docker info | Select-String "Operating System","OSType"
  ```
- [ ] NVIDIA Container Toolkit (for GPU workloads)
  ```bash
  # In WSL terminal
  nvidia-smi
  docker run --rm --gpus all nvidia/cuda:11.0-base nvidia-smi
  ```
- [ ] .NET SDK 9.0+
  ```powershell
  dotnet --version  # Should be 9.x or higher
  ```
- [ ] Ollama running on port 11434
  ```powershell
  curl http://localhost:11434/api/tags
  ```

### Directory Structure
- [ ] Required directories exist and are writable:
  ```powershell
  # Check core directories
  Test-Path data/
  Test-Path data/logs/
  Test-Path data/open-webui/
  Test-Path data/qdrant/
  Test-Path models/stable-diffusion/
  
  # Verify permissions
  (Get-Acl data).Access | Format-Table IdentityReference,FileSystemRights
  ```

## Core Service Verification

### Worker Service
- [ ] Build verification:
  ```powershell
  ./scripts/build.ps1
  # Should complete without errors
  ```

- [ ] Configuration loading:
  ```powershell
  # Check appsettings.json exists and is valid
  Get-Content SecureHomeSystem/appsettings.json | ConvertFrom-Json
  
  # Verify Development overrides
  Get-Content SecureHomeSystem/appsettings.Development.json | ConvertFrom-Json
  ```

- [ ] Health endpoint:
  ```powershell
  # Should return {"status":"Healthy"}
  curl http://localhost:5080/health
  ```

### Docker Integration
- [ ] Service detection:
  ```powershell
  # Check Docker socket access
  docker ps
  
  # Verify labels on running containers
  docker ps --format "{{.Names}}: {{.Labels}}"
  ```

- [ ] Log collection:
  ```powershell
  # Verify log directories
  Test-Path data/logs/services/
  Test-Path data/logs/state/
  
  # Check cursor files
  Get-ChildItem data/logs/state/*.cursor
  ```

## AI Services Verification

### OpenWebUI Integration
- [ ] Service health:
  ```powershell
  # Check OpenWebUI is running
  curl http://localhost:3000/api/system/info
  
  # Verify Ollama connection
  curl http://localhost:11434/api/tags
  ```

- [ ] Configuration:
  ```powershell
  # Check config file
  Get-Content data/open-webui/config.json
  
  # Verify environment variables
  docker compose exec open-webui env | Select-String "OLLAMA|WEBUI"
  ```

### Qdrant Integration
- [ ] Database health:
  ```powershell
  # Check Qdrant readiness
  curl http://localhost:6334/readyz
  
  # Verify collections directory
  Test-Path data/qdrant/collections/
  ```

### Stable Diffusion (Optional)
- [ ] GPU access:
  ```bash
  # Check GPU visibility
  docker compose exec automatic1111 nvidia-smi
  
  # Verify VRAM limits
  nvidia-smi -q -d MEMORY
  ```

- [ ] API functionality:
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
  ```

## Log Management Verification

### Collection System
- [ ] Log rotation:
  ```powershell
  # Check log sizes
  Get-ChildItem data/logs/services/ -Recurse | 
    Sort-Object Length -Descending | 
    Select-Object Name, Length
  
  # Verify rotation settings
  Get-Content SecureHomeSystem/appsettings.json | 
    Select-String -Context 2,2 "Rotation"
  ```

- [ ] Cursor management:
  ```powershell
  # Check cursor files
  Get-ChildItem data/logs/state/*.cursor | ForEach-Object {
    Get-Content $_.FullName
  }
  ```

## Resource Management Verification

### GPU Resources
- [ ] Resource limits:
  ```yaml
  # Verify in docker/compose.yaml
  automatic1111:
    environment:
      - NVIDIA_VISIBLE_DEVICES=${AUTOMATIC1111_GPU_SELECTION:-all}
    deploy:
      resources:
        reservations:
          devices:
            - driver: nvidia
              count: ${AUTOMATIC1111_GPU_COUNT:-all}
              capabilities: [gpu]
  ```

- [ ] Memory management:
  ```powershell
  # Check VRAM usage
  wsl nvidia-smi --query-gpu=memory.used,memory.total --format=csv
  
  # Verify Ollama limits
  Get-Content docker/.env | Select-String "OLLAMA_MAX_GPU_MEMORY"
  ```

## Integration Tests

### Smoke Tests
- [ ] Run full smoke test:
  ```powershell
  ./scripts/smoke.ps1
  ```

### Component Tests
- [ ] Run targeted tests:
  ```powershell
  dotnet test ShS.slnx --filter "Category=Unit"
  dotnet test ShS.slnx --filter "Category=Integration"
  ```

## Performance Baselines

### Response Times
- [ ] Health check latency:
  ```powershell
  # Measure response time
  Measure-Command { curl http://localhost:5080/health }
  ```

### Resource Usage
- [ ] Monitor GPU utilization:
  ```bash
  # Run in WSL
  nvidia-smi dmon -s pucvt -i 0
  ```

- [ ] Container stats:
  ```powershell
  docker stats --no-stream
  ```

## Security Verification

### Authentication
- [ ] OpenWebUI auth:
  ```powershell
  # Verify auth requirement
  curl -I http://localhost:3000/api/auth/session
  ```

### File Permissions
- [ ] Check sensitive directories:
  ```powershell
  # Verify model directory permissions
  (Get-Acl models/stable-diffusion).Access
  
  # Check log directory permissions
  (Get-Acl data/logs).Access
  ```

## Recovery Procedures

### Service Recovery
- [ ] Test service restart:
  ```powershell
  docker compose restart shs-worker
  # Verify logs show clean startup
  ```

### Data Recovery
- [ ] Verify backup locations:
  ```powershell
  # Check critical paths
  Test-Path data/qdrant/collections/
  Test-Path data/open-webui/vector_db/
  Test-Path models/stable-diffusion/*.safetensors
  ```