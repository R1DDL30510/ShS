# System Integration Verification

This checklist verifies the integration points and data flows between components in the SecureHomeSystem stack.

## Service Communication Patterns

### Worker → Docker
- [ ] Docker Socket Communication
  ```powershell
  # Verify socket mount
  docker compose config | Select-String "docker.sock"
  
  # Test Docker CLI access
  docker compose exec shs-worker docker ps
  ```

### Worker → Service Discovery
- [ ] Label Detection
  ```powershell
  # Check service labels
  docker ps --format "{{.Names}}: {{.Labels}}"
  
  # Verify in logs
  docker compose logs shs-worker | Select-String "Detected service"
  ```

### OpenWebUI → Ollama
- [ ] LLM Integration
  ```powershell
  # Check Ollama connection
  curl http://localhost:3000/api/system/info
  
  # Verify model access
  curl http://localhost:11434/api/tags
  ```

### OpenWebUI → Qdrant
- [ ] Vector Database Integration
  ```powershell
  # Check Qdrant health
  curl http://localhost:6334/readyz
  
  # Verify collections
  ls data/qdrant/collections/
  ```

## Data Flow Verification

### Log Collection Flow
1. [ ] Container → Worker
   ```powershell
   # Check log collection
   docker compose logs shs-worker | Select-String "Collected.*log entries"
   ```

2. [ ] Worker → Storage
   ```powershell
   # Verify log files
   Get-ChildItem data/logs/services/*.log
   
   # Check cursors
   Get-ChildItem data/logs/state/*.cursor
   ```

3. [ ] Rotation System
   ```powershell
   # Check rotation triggers
   Get-Content SecureHomeSystem/appsettings.json | Select-String -Context 2,2 "Rotation"
   
   # Verify archived logs
   Get-ChildItem data/logs/services/*.log.* | Sort-Object LastWriteTime
   ```

### Model Data Flow
1. [ ] Model Loading
   ```powershell
   # Check model files
   Get-ChildItem models/stable-diffusion/*.safetensors
   
   # Verify mounting
   docker compose config | Select-String "models/stable-diffusion"
   ```

2. [ ] Model Access
   ```powershell
   # Test Ollama models
   curl http://localhost:11434/api/tags
   
   # Check SD models
   curl http://localhost:7860/sdapi/v1/sd-models
   ```

## State Management

### Persistence Verification
1. [ ] Vector Database
   ```powershell
   # Check Qdrant persistence
   Test-Path data/qdrant/collections/
   Get-ChildItem data/qdrant/collections/
   ```

2. [ ] Configuration State
   ```powershell
   # Verify OpenWebUI config
   Test-Path data/open-webui/config.json
   Get-Content data/open-webui/config.json
   ```

3. [ ] Log State
   ```powershell
   # Check cursor persistence
   Get-ChildItem data/logs/state/*.cursor | ForEach-Object {
     $name = $_.Name
     $content = Get-Content $_.FullName
     Write-Host "${name}: ${content}"
   }
   ```

## Cross-Component Scenarios

### Full Stack Verification
1. [ ] Stack Startup
   ```powershell
   # Start core stack
   docker compose --profile worker up -d
   
   # Verify all services
   docker compose ps
   ```

2. [ ] Health Status
   ```powershell
   # Check all health endpoints
   curl http://localhost:5080/health
   curl http://localhost:3000/api/system/info
   curl http://localhost:6334/readyz
   ```

3. [ ] GPU Access
   ```bash
   # Check GPU visibility
   nvidia-smi
   
   # Verify container access
   docker compose exec automatic1111 nvidia-smi
   ```

### Resource Coordination
1. [ ] GPU Management
   ```powershell
   # Check resource limits
   Get-Content docker/.env | Select-String "GPU|NVIDIA"
   
   # Monitor usage
   wsl watch -n1 nvidia-smi
   ```

2. [ ] Memory Usage
   ```powershell
   # Check container memory
   docker stats --no-stream
   
   # Verify limits
   docker compose config | Select-String "memory"
   ```

## Recovery Scenarios

### Service Recovery
1. [ ] Worker Restart
   ```powershell
   # Stop worker
   docker compose stop shs-worker
   
   # Verify log collection stops
   Get-ChildItem data/logs/services/ -Filter *.log |
     Sort-Object LastWriteTime -Descending |
     Select-Object Name, LastWriteTime -First 1
   
   # Restart worker
   docker compose start shs-worker
   
   # Verify collection resumes
   docker compose logs -f shs-worker
   ```

2. [ ] Component Restart
   ```powershell
   # Test OpenWebUI restart
   docker compose restart open-webui
   
   # Verify detection
   docker compose logs shs-worker | Select-String "Detected service.*open-webui"
   ```

### Data Recovery
1. [ ] Log Recovery
   ```powershell
   # Simulate log loss
   Rename-Item data/logs/services/open-webui.log open-webui.log.bak
   
   # Verify recreation
   docker compose restart shs-worker
   Test-Path data/logs/services/open-webui.log
   ```

2. [ ] State Recovery
   ```powershell
   # Test cursor recovery
   Move-Item data/logs/state/open-webui.cursor open-webui.cursor.bak
   
   # Verify recreation
   docker compose restart shs-worker
   Test-Path data/logs/state/open-webui.cursor
   ```

## End-to-End Tests

### Smoke Test Sequence
```powershell
# Full verification
./scripts/smoke.ps1

# Check test results
Get-Content tests/TestResults/*/coverage.cobertura.xml
```

### Integration Test Suite
```powershell
# Run integration tests
dotnet test ShS.slnx --filter "Category=Integration"
```

## Monitoring Integration

### Log Analysis
1. [ ] Format Verification
   ```powershell
   # Check JSON formatting
   Get-Content data/logs/worker/*.json | ConvertFrom-Json
   
   # Verify log structure
   Get-Content data/logs/services/*.log | Select-Object -First 10
   ```

2. [ ] Metric Collection
   ```powershell
   # GPU metrics
   wsl nvidia-smi --query-gpu=utilization.gpu,memory.used,memory.free --format=csv
   
   # Container stats
   docker stats --no-stream
   ```

## Security Integration

### Authentication Flow
1. [ ] OpenWebUI Auth
   ```powershell
   # Check auth requirement
   curl -I http://localhost:3000/api/auth/session
   
   # Verify config
   docker compose config | Select-String "WEBUI_AUTH"
   ```

2. [ ] API Security
   ```powershell
   # Test secured endpoints
   curl -I http://localhost:7860/sdapi/v1/txt2img
   ```

### Access Control
1. [ ] File Permissions
   ```powershell
   # Check sensitive paths
   (Get-Acl models/stable-diffusion).Access
   (Get-Acl data/logs).Access
   ```

2. [ ] Container Isolation
   ```powershell
   # Verify resource isolation
   docker compose config | Select-String "deploy.*resources"
   ```