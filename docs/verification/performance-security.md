# Performance and Security Verification

This checklist covers performance baselines, security considerations, and operational thresholds for the SecureHomeSystem stack.

## Performance Baselines

### Response Time Benchmarks

#### Worker Service
```powershell
# Health endpoint latency
1..10 | ForEach-Object {
    $result = Measure-Command { 
        curl -s http://localhost:5080/health > $null
    }
    Write-Host "Request $_ : $($result.TotalMilliseconds)ms"
}

# Expected: < 100ms average
```

#### OpenWebUI
```powershell
# System info latency
1..10 | ForEach-Object {
    $result = Measure-Command {
        curl -s http://localhost:3000/api/system/info > $null
    }
    Write-Host "Request $_ : $($result.TotalMilliseconds)ms"
}

# Expected: < 200ms average
```

#### Qdrant
```powershell
# Health check latency
1..10 | ForEach-Object {
    $result = Measure-Command {
        curl -s http://localhost:6334/readyz > $null
    }
    Write-Host "Request $_ : $($result.TotalMilliseconds)ms"
}

# Expected: < 50ms average
```

### Resource Utilization

#### GPU Monitoring
```bash
# Baseline GPU stats
nvidia-smi --query-gpu=utilization.gpu,memory.used,memory.free,temperature.gpu --format=csv -l 1

# Expected ranges:
# - Idle: < 5% GPU, < 2GB VRAM
# - Normal: 20-50% GPU, 4-8GB VRAM
# - Peak: < 80% GPU, < 80% VRAM
```

#### Memory Usage
```powershell
# Container memory baselines
docker stats --no-stream --format "table {{.Name}}\t{{.CPUPerc}}\t{{.MemUsage}}\t{{.MemPerc}}"

# Expected ranges:
# - shs-worker: < 200MB
# - open-webui: < 1GB
# - qdrant: < 500MB
# - automatic1111: < 8GB
```

#### Disk I/O
```powershell
# Log write performance
$testFile = "data/logs/services/perf-test.log"
$size = 1MB

# Write test
$writeTime = Measure-Command {
    1..$size | ForEach-Object { "Test log entry $_" } | Out-File $testFile
}
Write-Host "Write throughput: $([math]::Round($size/$writeTime.TotalSeconds,2)) MB/s"

# Expected: > 50 MB/s
Remove-Item $testFile
```

## Security Verification

### Authentication Checks

#### OpenWebUI Security
```powershell
# Verify auth requirement
$response = curl -I http://localhost:3000/api/auth/session
if ($response -match "401") {
    Write-Host "Auth check passed: Unauthorized without credentials"
} else {
    Write-Host "Auth check failed: Endpoint accessible without auth"
}

# Test invalid credentials
$response = curl -I -H "Authorization: Bearer invalid" http://localhost:3000/api/auth/session
if ($response -match "401") {
    Write-Host "Invalid token check passed"
} else {
    Write-Host "Invalid token check failed"
}
```

#### API Security
```powershell
# Check exposed endpoints
$endpoints = @(
    "http://localhost:5080/health",
    "http://localhost:3000/api/system/info",
    "http://localhost:6334/readyz",
    "http://localhost:7860/sdapi/v1/txt2img"
)

foreach ($endpoint in $endpoints) {
    $response = curl -I $endpoint
    Write-Host "Endpoint $endpoint : $response"
}
```

### File System Security

#### Permission Verification
```powershell
# Check critical directories
$paths = @(
    "data/logs",
    "data/open-webui",
    "data/qdrant",
    "models/stable-diffusion"
)

foreach ($path in $paths) {
    $acl = Get-Acl $path
    Write-Host "=== $path Permissions ==="
    $acl.Access | Format-Table IdentityReference,FileSystemRights
}
```

#### Sensitive File Protection
```powershell
# Verify log file permissions
Get-ChildItem data/logs/services/*.log | ForEach-Object {
    $acl = Get-Acl $_
    Write-Host "=== $($_.Name) Permissions ==="
    $acl.Access | Where-Object { $_.FileSystemRights -match "Write|Modify|FullControl" }
}
```

## Performance Thresholds

### Resource Limits

#### GPU Thresholds
```powershell
# Check configured limits
Get-Content docker/.env | Select-String "GPU|NVIDIA"

# Verify in config
Get-Content SecureHomeSystem/appsettings.json | Select-String -Context 2,2 "ResourceScheduler"

# Expected:
# - OLLAMA_MAX_GPU_MEMORY=0.8
# - GpuUtilisationThreshold=0.5
# - GpuMemoryThreshold=0.8
```

#### Memory Thresholds
```powershell
# Container memory limits
docker compose config | Select-String -Context 2,2 "memory"

# Monitor usage vs limits
docker stats --no-stream --format "{{.Name}}: {{.MemPerc}}"
```

### Performance Monitoring

#### Response Time Monitoring
```powershell
# Create monitoring function
function Test-Endpoint {
    param($url, $count=60)
    $times = 1..$count | ForEach-Object {
        $result = Measure-Command { 
            curl -s $url > $null 
        }
        Start-Sleep -Seconds 1
        $result.TotalMilliseconds
    }
    $avg = ($times | Measure-Object -Average).Average
    $max = ($times | Measure-Object -Maximum).Maximum
    $min = ($times | Measure-Object -Minimum).Minimum
    Write-Host "URL: $url"
    Write-Host "Avg: ${avg}ms, Min: ${min}ms, Max: ${max}ms"
}

# Monitor key endpoints
Test-Endpoint "http://localhost:5080/health"
Test-Endpoint "http://localhost:3000/api/system/info"
Test-Endpoint "http://localhost:6334/readyz"
```

#### Resource Monitoring
```bash
# GPU monitoring script
watch -n 5 'nvidia-smi --query-gpu=utilization.gpu,memory.used,memory.total,temperature.gpu --format=csv'

# Expected thresholds:
# - GPU Utilization: < 80%
# - Memory Usage: < 80%
# - Temperature: < 80°C
```

### Log Analysis

#### Log Volume
```powershell
# Monitor log growth
function Get-LogGrowth {
    param($path, $minutes=5)
    $initial = (Get-ChildItem $path -Recurse | Measure-Object Length -Sum).Sum
    Start-Sleep -Seconds ($minutes * 60)
    $final = (Get-ChildItem $path -Recurse | Measure-Object Length -Sum).Sum
    $rate = ($final - $initial) / (1MB * $minutes)
    Write-Host "Log growth rate: $rate MB/minute"
}

# Check service logs
Get-LogGrowth "data/logs/services"

# Expected: < 1 MB/minute average
```

#### Log Rotation Performance
```powershell
# Check rotation timing
Get-ChildItem data/logs/services/*.log.* | 
    Sort-Object LastWriteTime | 
    Select-Object Name, 
                 @{N='Size(MB)';E={$_.Length/1MB}},
                 LastWriteTime

# Verify rotation doesn't impact service
docker compose logs --since 5m shs-worker | 
    Select-String "Rotated service log"
```

## Security Hardening

### Network Security

#### Port Exposure
```powershell
# Check exposed ports
docker compose ps --format "{{.Names}}: {{.Ports}}"

# Verify against expected
$expected = @{
    "shs-worker" = "5080"
    "open-webui" = "3000"
    "qdrant" = "6334"
    "automatic1111" = "7860"
}

foreach ($svc in $expected.Keys) {
    $ports = docker compose ps --format "{{.Ports}}" $svc
    Write-Host "${svc}: $ports"
}
```

#### Container Isolation
```powershell
# Check network isolation
docker network ls
docker network inspect shs-stack_default

# Verify container capabilities
docker compose config | Select-String -Context 2,2 "capabilities"
```

### Access Control

#### File Access Patterns
```powershell
# Monitor file access
Get-ChildItem data/logs/services/*.log | ForEach-Object {
    $file = $_
    Write-Host "=== $($file.Name) Access Times ==="
    Write-Host "Created: $($file.CreationTime)"
    Write-Host "Modified: $($file.LastWriteTime)"
    Write-Host "Accessed: $($file.LastAccessTime)"
}
```

#### Process Isolation
```powershell
# Check process isolation
docker compose top

# Verify user contexts
docker compose exec -u root shs-worker id
docker compose exec -u root open-webui id
```

## Operational Thresholds

### Service Health

#### Response Time Thresholds
```powershell
# Define thresholds
$thresholds = @{
    "worker" = @{url="http://localhost:5080/health"; max=100}
    "webui" = @{url="http://localhost:3000/api/system/info"; max=200}
    "qdrant" = @{url="http://localhost:6334/readyz"; max=50}
}

# Test against thresholds
foreach ($svc in $thresholds.Keys) {
    $result = Measure-Command { 
        curl -s $thresholds[$svc].url > $null
    }
    $ms = $result.TotalMilliseconds
    $max = $thresholds[$svc].max
    Write-Host "${svc}: ${ms}ms (max: ${max}ms) : $(if($ms -le $max){'OK'}else{'WARNING'})"
}
```

#### Resource Thresholds
```powershell
# Memory thresholds
$memThresholds = @{
    "shs-worker" = 200
    "open-webui" = 1000
    "qdrant" = 500
    "automatic1111" = 8000
}

# Check against thresholds
docker stats --no-stream --format "{{.Name}}\t{{.MemUsage}}" | ForEach-Object {
    $name, $mem = $_ -split "\t"
    $used = [int]($mem -replace '[^0-9]')
    $threshold = $memThresholds[$name]
    Write-Host "${name}: ${used}MB/$(${threshold})MB : $(if($used -le $threshold){'OK'}else{'WARNING'})"
}
```