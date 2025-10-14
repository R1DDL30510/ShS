# Container Orchestration Validation

This document outlines validation procedures for container orchestration, lifecycle management, and inter-service communication within the SecureHomeSystem stack.

## Container Lifecycle Validation

### 1. Startup Sequence

```powershell
# Clean environment
docker compose down -v
docker system prune -f

# Start core services
docker compose --profile worker up -d
$startTime = Get-Date

# Monitor service initialization
docker compose logs -f --tail=100 shs-worker | ForEach-Object {
    $line = $_
    $elapsed = ((Get-Date) - $startTime).TotalSeconds
    Write-Host ("{0:F1}s: {1}" -f $elapsed, $line)
}

# Expected sequence:
# 1. Worker initializes and logs endpoints
# 2. OpenWebUI detected and running
# 3. Qdrant detected and running
# 4. Log collection starts if enabled
```

### 2. Container Dependencies

```powershell
# Verify service dependencies
docker compose config --services | ForEach-Object {
    $service = $_
    $deps = docker compose config | Select-String "depends_on:" -Context 0,5 | 
            Where-Object { $_.Context.PreContext -match $service }
    Write-Host "`nService: $service"
    Write-Host "Dependencies: $($deps.Context.PostContext -join ',')"
}

# Expected dependencies:
# - open-webui depends on qdrant
# - shs-worker independent (monitors others)
```

### 3. Health Check Validation

```powershell
# Verify health check configurations
docker compose config | Select-String "healthcheck:" -Context 0,5

# Test health endpoints
$endpoints = @{
    "worker" = "http://localhost:5080/health"
    "webui" = "http://localhost:3000/api/system/info"
    "qdrant" = "http://localhost:6334/readyz"
}

foreach ($svc in $endpoints.Keys) {
    $response = try {
        Invoke-WebRequest -Uri $endpoints[$svc] -Method GET
        "OK ($($response.StatusCode))"
    } catch {
        "Failed: $($_.Exception.Message)"
    }
    Write-Host "${svc}: $response"
}
```

### 4. Restart Behavior

```powershell
# Test service recovery
foreach ($svc in @("open-webui", "qdrant", "shs-worker")) {
    Write-Host "`nTesting restart of $svc..."
    
    # Stop service
    docker compose stop $svc
    Start-Sleep -Seconds 5
    
    # Verify it's stopped
    $status = docker compose ps $svc --format json | ConvertFrom-Json
    Write-Host "After stop: $($status.State)"
    
    # Restart service
    docker compose start $svc
    Start-Sleep -Seconds 10
    
    # Verify recovery
    $status = docker compose ps $svc --format json | ConvertFrom-Json
    Write-Host "After restart: $($status.State)"
    
    # Check logs for recovery
    docker compose logs --since=30s $svc
}

# Expected behavior:
# - Services should restart automatically
# - Worker should detect state changes
# - Logs should show clean recovery
```

## Service Communication

### 1. Network Isolation

```powershell
# Inspect network configuration
docker network ls --filter name=shs-stack
docker network inspect shs-stack_default

# Verify container connectivity
foreach ($svc in @("shs-worker", "open-webui", "qdrant")) {
    Write-Host "`nConnectivity for $svc:"
    docker compose exec $svc sh -c '
        nc -zv qdrant 6334;
        nc -zv open-webui 3000;
        nc -zv shs-worker 5080
    '
}

# Expected:
# - Services in same network can communicate
# - External access only through published ports
```

### 2. Service Discovery

```powershell
# Monitor service detection
docker compose logs -f shs-worker | Select-String "Detected service"

# Verify label-based discovery
docker compose ps --format json | ConvertFrom-Json | ForEach-Object {
    Write-Host "`nContainer: $($_.Name)"
    Write-Host "Labels:"
    docker inspect $_.ID --format '{{.Config.Labels}}' | ForEach-Object {
        $_ -split ' ' | Where-Object { $_ -match 'shs.role=' }
    }
}

# Expected:
# - All services have shs.role label
# - Worker detects services via labels
```

### 3. Log Collection Flow

```powershell
# Generate test logs
$services = @("open-webui", "qdrant")
foreach ($svc in $services) {
    docker compose exec $svc sh -c 'echo "Test log entry from $HOSTNAME"'
}

# Monitor log collection
docker compose logs -f shs-worker | Select-String "Collected.*log entries"

# Verify log files
Get-ChildItem data/logs/services/*.log | ForEach-Object {
    Write-Host "`nLog file: $($_.Name)"
    Get-Content $_ -Tail 5
}

# Expected:
# - Log files created for each service
# - Entries have timestamps and metadata
# - No duplicate entries across restarts
```

### 4. Resource Sharing

```powershell
# Check resource limits
docker compose config | Select-String -Pattern "(mem_limit|cpus|gpu):" -Context 2,2

# Monitor resource usage
docker stats --no-stream --format "table {{.Name}}\t{{.CPUPerc}}\t{{.MemUsage}}\t{{.NetIO}}\t{{.BlockIO}}"

# For GPU workloads
if (Get-Command nvidia-smi -ErrorAction SilentlyContinue) {
    nvidia-smi --query-compute-apps=pid,process_name,used_memory --format=csv
}

# Expected:
# - Resource limits respected
# - Network I/O shows service communication
# - GPU properly shared when enabled
```

## Failure Scenarios

### 1. Network Partition

```powershell
# Simulate network issues
docker network disconnect shs-stack_default shs-stack-open-webui-1
Start-Sleep -Seconds 10

# Check detection and logging
docker compose logs --since=20s shs-worker

# Restore connectivity
docker network connect shs-stack_default shs-stack-open-webui-1
Start-Sleep -Seconds 10

# Verify recovery
docker compose logs --since=20s shs-worker

# Expected:
# - Worker detects service disconnection
# - Service recovers after reconnection
```

### 2. Container Crashes

```powershell
# Simulate crash
docker compose exec open-webui sh -c 'kill 1'
Start-Sleep -Seconds 10

# Check restart policy
docker compose ps open-webui --format json | ConvertFrom-Json | 
    Select-Object Name, State, Health

# Monitor recovery
docker compose logs --since=30s open-webui

# Expected:
# - Container automatically restarts
# - Worker detects status changes
# - Service recovers to healthy state
```

### 3. Resource Exhaustion

```powershell
# Monitor under load
$testDuration = 30
$startTime = Get-Date

Write-Host "Starting load test for ${testDuration}s..."

# Generate load
1..$testDuration | ForEach-Object {
    $elapsed = ((Get-Date) - $startTime).TotalSeconds
    $stats = docker stats --no-stream --format "json"
    
    $stats | ConvertFrom-Json | ForEach-Object {
        Write-Host ("{0:F1}s - {1}: CPU: {2}, Mem: {3}" -f $elapsed, 
            $_.Name, $_.CPUPerc, $_.MemUsage)
    }
    
    Start-Sleep -Seconds 1
}

# Expected:
# - Resource limits prevent cascading failures
# - Services remain responsive under load
```

## Service State Verification

### 1. State Consistency

```powershell
# Verify consistent state reporting
$verifyState = {
    $workerLogs = docker compose logs --tail=50 shs-worker | 
        Select-String "Detected service" | 
        Select-Object -Last 1

    $containerStates = docker compose ps --format json | 
        ConvertFrom-Json | 
        Select-Object Name, State, Health

    Write-Host "`nWorker detected state:"
    Write-Host $workerLogs

    Write-Host "`nActual container states:"
    $containerStates | Format-Table
}

# Check initial state
& $verifyState

# After service restart
docker compose restart open-webui
Start-Sleep -Seconds 10
& $verifyState

# Expected:
# - Worker state matches container state
# - Health status consistent
```

### 2. Endpoint Availability

```powershell
# Define endpoints
$endpoints = @{
    "worker" = @{
        "health" = "http://localhost:5080/health"
        "live" = "http://localhost:5080/live"
    }
    "webui" = @{
        "info" = "http://localhost:3000/api/system/info"
        "health" = "http://localhost:3000/health"
    }
    "qdrant" = @{
        "ready" = "http://localhost:6334/readyz"
        "live" = "http://localhost:6334/livez"
    }
}

# Test all endpoints
foreach ($svc in $endpoints.Keys) {
    Write-Host "`nTesting $svc endpoints:"
    foreach ($endpoint in $endpoints[$svc].Keys) {
        $url = $endpoints[$svc][$endpoint]
        try {
            $response = Invoke-WebRequest -Uri $url -Method GET
            Write-Host "$endpoint : OK ($($response.StatusCode))"
        } catch {
            Write-Host "$endpoint : Failed - $($_.Exception.Message)"
        }
    }
}

# Expected:
# - All health endpoints respond
# - Status codes indicate healthy state
```

## Verification Checklist

### Basic Functionality
- [ ] Clean startup sequence completes
- [ ] Service dependencies respected
- [ ] Health checks passing
- [ ] Resource limits configured

### Communication
- [ ] Network isolation verified
- [ ] Service discovery working
- [ ] Log collection flowing
- [ ] Resource sharing effective

### Resilience
- [ ] Network partition recovery
- [ ] Crash recovery
- [ ] Resource exhaustion handling
- [ ] State consistency maintained

### Monitoring
- [ ] Worker detection accurate
- [ ] Container health reported
- [ ] Log collection verified
- [ ] Resource usage tracked

### Documentation
- [ ] Container roles documented
- [ ] Network topology mapped
- [ ] Recovery procedures tested
- [ ] Resource limits validated