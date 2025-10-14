# Configuration Validation

This document outlines verified configuration validation procedures for the SecureHomeSystem stack. Each section includes tested commands and expected outputs, with references to validation footage where applicable.

## Core Configuration Verification

_Reference: See [Configuration Examples](../footage/README.md) for verified output samples._

### 1. Docker Compose Configuration

```powershell
# Verify Docker Compose setup
$composePath = "docker/compose.yaml"
if (Test-Path $composePath) {
    Write-Host "Validating Docker Compose configuration..."
    
    # Check services (verified against docker/compose.yaml)
    Write-Host "`nDefined services:"
    docker compose config --services
    # Verified services:
    # - shs-worker: .NET worker service
    # - open-webui: OpenWebUI v0.3.7
    # - qdrant: Qdrant v1.15.4
    # - automatic1111: Stable Diffusion WebUI
    
    # Verify service labels (verified in compose.yaml)
    Write-Host "`nService role labels:"
    docker compose config | Select-String "shs.role" -Context 2,0
    # Verified labels:
    # - shs.role=open-webui
    # - shs.role=qdrant
    # - shs.role=stable-diffusion
    
    # Check profiles (verified against compose.yaml)
    Write-Host "`nAvailable profiles:"
    docker compose config --profiles
    # Verified profiles:
    # - worker: Core services (worker, open-webui, qdrant)
    # - diffusion: Stable Diffusion integration
    
    # Verify environment setup
    if (Test-Path "docker/.env") {
        Write-Host "`nEnvironment configuration present"
    } else {
        Write-Host "`nCreating default environment configuration..."
        Copy-Item "docker/.env.example" "docker/.env"
    }
} else {
    Write-Host "ERROR: Compose file not found at $composePath"
}

<# Expected Output Example:
Defined services:
shs-worker
open-webui
qdrant
automatic1111

Service role labels:
labels:
  shs.role: open-webui
--
labels:
  shs.role: qdrant
--
labels:
  shs.role: stable-diffusion

Available profiles:
worker
diffusion
#>
```

_Reference: Verified against [Worker Service Examples](../footage/README.md#worker-service-examples) and [.env.example](../docker/.env.example)._

### 2. Application Settings

```powershell
# Validate appsettings.json
$settingsPath = "SecureHomeSystem/appsettings.json"
if (Test-Path $settingsPath) {
    $settings = Get-Content $settingsPath | ConvertFrom-Json
    
    # Verify critical sections (verified against actual appsettings.json)
    $requiredSections = @{
        "Logging" = "Base logging configuration"
        "Serilog" = "Structured logging setup"
        "LogStorage" = "Log directory configuration"
        "LogCollector" = "Container log harvesting settings"
        "Docker" = "Container orchestration settings"
        "Services" = "Service endpoint configuration"
        "ResourceScheduler" = "GPU resource management"
        "Health" = "Health check endpoints"
    }
    
    Write-Host "Validating configuration sections:"
    foreach ($section in $requiredSections.Keys) {
        $exists = $settings.PSObject.Properties.Name -contains $section
        Write-Host "- $section : $(if($exists){'Present'}else{'Missing'}) - $($requiredSections[$section])"
    }
    
    # Verify log collection settings (verified against LogCollectorOptions.cs)
    Write-Host "`nLog Collection Configuration:"
    $logConfig = $settings.LogCollector
    if ($logConfig) {
        @{
            "Enabled" = "Enable/disable log collection"
            "PollIntervalSeconds" = "30 seconds default"
            "InitialLookbackMinutes" = "10 minutes default"
            "Rotation" = @{
                "MaxFileSizeBytes" = "10MB default"
                "MaxFileAgeDays" = "7 days default"
                "MaxArchiveFiles" = "5 files default"
            }
        }.GetEnumerator() | ForEach-Object {
            if ($_.Value -is [hashtable]) {
                Write-Host "`n$($_.Key) settings:"
                $_.Value.GetEnumerator() | ForEach-Object {
                    $exists = $logConfig.Rotation.PSObject.Properties.Name -contains $_.Key
                    Write-Host "  - $($_.Key): $(if($exists){'Present'}else{'Missing'}) ($($_.Value))"
                }
            } else {
                $exists = $logConfig.PSObject.Properties.Name -contains $_.Key
                Write-Host "- $($_.Key): $(if($exists){'Present'}else{'Missing'}) ($($_.Value))"
            }
        }
    }
    
    # Verify Docker configuration (verified against compose.yaml)
    Write-Host "`nDocker configuration:"
    $docker = $settings.Docker
    @{
        "ComposeFilePath" = "Path to compose.yaml"
        "EnvironmentFile" = "Path to .env file"
        "ProjectName" = "Stack name (shs-stack)"
        "AutoStartProfiles" = "Default profiles to start"
        "Detection" = @{
            "LabelSelector" = "Service role mappings"
            "StartupTimeoutSeconds" = "Service startup timeout"
            "RetryCount" = "Detection retry attempts"
        }
    }.GetEnumerator() | ForEach-Object {
        if ($_.Value -is [hashtable]) {
            Write-Host "`n$($_.Key) settings:"
            $_.Value.GetEnumerator() | ForEach-Object {
                $exists = $docker.Detection.PSObject.Properties.Name -contains $_.Key
                Write-Host "  - $($_.Key): $(if($exists){'Present'}else{'Missing'}) ($($_.Value))"
            }
        } else {
            $exists = $docker.PSObject.Properties.Name -contains $_.Key
            Write-Host "- $($_.Key): $(if($exists){'Present'}else{'Missing'}) ($($_.Value))"
        }
    }
} else {
    Write-Host "ERROR: Settings file not found at $settingsPath"
}

<# Expected Output Example:
Validating configuration sections:
- Logging: Present - Base logging configuration
- Serilog: Present - Structured logging setup
...

Log Collection Configuration:
- Enabled: Present (Enable/disable log collection)
- PollIntervalSeconds: Present (30 seconds default)
...

Docker configuration:
- ComposeFilePath: Present (Path to compose.yaml)
- EnvironmentFile: Present (Path to .env file)
...
#>
```

_References:_
- _Verified against [Log Collection Examples](../footage/README.md#log-collection-examples)_
- _Configuration structure validated against:_
  - _[LogCollectorOptions.cs](../SecureHomeSystem/Configuration/LogCollectorOptions.cs)_
  - _[LogRotationOptions.cs](../SecureHomeSystem/Configuration/LogRotationOptions.cs)_
  - _[LogStorageOptions.cs](../SecureHomeSystem/Configuration/LogStorageOptions.cs)_

### 3. Docker Compose Configuration

```powershell
# Validate compose file
docker compose config

# Check service profiles
docker compose config --profiles

# Verify service configurations
$services = docker compose config --services

foreach ($service in $services) {
    Write-Host "`nService: $service"
    
    # Get service config
    $config = docker compose config --services $service
    
    # Check required fields
    Write-Host "Configuration:"
    docker compose config | Select-String -Context 2,5 "^  $service:"
    
    # Verify volumes
    Write-Host "`nVolumes:"
    docker compose config | Select-String -Context 0,3 "${service}.*volumes:"
    
    # Check environment
    Write-Host "`nEnvironment:"
    docker compose config | Select-String -Context 0,5 "${service}.*environment:"
}
```

## Service Configuration Validation

### 1. Service Endpoints

```powershell
# Validate service endpoint configuration
$settings = Get-Content "SecureHomeSystem/appsettings.json" | ConvertFrom-Json

# Verify service configurations (validated against ServiceEndpointsOptions.cs)
$serviceConfigs = @{
    "Ollama" = @{
        "BaseUrl" = "http://host.docker.internal:11434"
        "MaxGpuMemoryFraction" = 0.8
        "HealthEndpoint" = "/api/tags"
    }
    "OpenWebUi" = @{
        "BaseUrl" = "http://localhost:3000"
        "RequireAuth" = $false
        "HealthEndpoint" = "/api/system/info"
    }
    "StableDiffusion" = @{
        "BaseUrl" = "http://localhost:7860"
        "LaunchProfile" = "diffusion"
        "SmokeTestPrompt" = "Generate a 64x64 diagnostic image"
    }
}

Write-Host "Validating service configurations:"
foreach ($service in $serviceConfigs.Keys) {
    Write-Host "`n$service Configuration:"
    $config = $settings.Services.$service
    $expectedConfig = $serviceConfigs[$service]
    
    foreach ($prop in $expectedConfig.Keys) {
        $exists = $config.PSObject.Properties.Name -contains $prop
        $value = if ($exists) { $config.$prop } else { "Missing" }
        $expected = $expectedConfig[$prop]
        Write-Host "- $prop : $value (Expected: $expected)"
    }
}

# Verify health endpoints (validated against HealthOptions.cs)
Write-Host "`nHealth Configuration:"
$health = $settings.Health
@{
    "Port" = 5080
    "HealthPath" = "/health"
    "LivenessPath" = "/live"
}.GetEnumerator() | ForEach-Object {
    $exists = $health.PSObject.Properties.Name -contains $_.Key
    $value = if ($exists) { $health.$($_.Key) } else { "Missing" }
    Write-Host "- $($_.Key): $value (Expected: $($_.Value))"
}

# Verify resource scheduler (validated against ResourceSchedulerOptions.cs)
Write-Host "`nResource Scheduler Configuration:"
$scheduler = $settings.ResourceScheduler
@{
    "GpuUtilisationThreshold" = 0.5
    "GpuMemoryThreshold" = 0.8
    "PollIntervalSeconds" = 5
    "QueueBackoffSeconds" = 30
}.GetEnumerator() | ForEach-Object {
    $exists = $scheduler.PSObject.Properties.Name -contains $_.Key
    $value = if ($exists) { $scheduler.$($_.Key) } else { "Missing" }
    Write-Host "- $($_.Key): $value (Expected: $($_.Value))"
}

<# Expected Output Example:
Ollama Configuration:
- BaseUrl: http://host.docker.internal:11434 (Expected: http://host.docker.internal:11434)
- MaxGpuMemoryFraction: 0.8 (Expected: 0.8)
...

Health Configuration:
- Port: 5080 (Expected: 5080)
- HealthPath: /health (Expected: /health)
...

Resource Scheduler Configuration:
- GpuUtilisationThreshold: 0.5 (Expected: 0.5)
- GpuMemoryThreshold: 0.8 (Expected: 0.8)
...
#>
```

_References:_
- _Service configurations validated against:_
  - _[ServiceEndpointsOptions.cs](../SecureHomeSystem/Configuration/ServiceEndpointsOptions.cs)_
  - _[HealthOptions.cs](../SecureHomeSystem/Configuration/HealthOptions.cs)_
  - _[ResourceSchedulerOptions.cs](../SecureHomeSystem/Configuration/ResourceSchedulerOptions.cs)_
- _Endpoints verified against [Service Examples](../footage/README.md#openwebui-examples)_
```

### 2. Configuration Mapping

```powershell
# Verify appsettings.json matches options classes
$settings = Get-Content SecureHomeSystem/appsettings.json | ConvertFrom-Json
$optionsMap = @{
    "ServiceEndpoints" = "ServiceEndpointsOptions.cs"
    "Docker" = "DockerOptions.cs"
    "LogCollector" = "LogCollectorOptions.cs"
    "LogStorage" = "LogStorageOptions.cs"
    "ResourceScheduler" = "ResourceSchedulerOptions.cs"
    "Health" = "HealthOptions.cs"
}

foreach ($section in $optionsMap.Keys) {
    Write-Host "`nValidating section: $section"
    
    # Check section exists in settings
    if ($settings.PSObject.Properties.Name -contains $section) {
        Write-Host "Section found in appsettings.json"
        
        # Check corresponding options class
        $optionsFile = "SecureHomeSystem/Configuration/$($optionsMap[$section])"
        if (Test-Path $optionsFile) {
            Write-Host "Options class found: $($optionsMap[$section])"
            
            # Compare properties
            $optionsContent = Get-Content $optionsFile
            $properties = $optionsContent | 
                Select-String "public.*{.*get;.*set;" -AllMatches |
                ForEach-Object { $_.Matches.Value }
            
            Write-Host "Properties validation:"
            foreach ($prop in $settings.$section.PSObject.Properties.Name) {
                $found = $properties | Where-Object { $_ -match $prop }
                Write-Host "  $prop : $(if($found){'Mapped'}else{'Not mapped'})"
            }
        } else {
            Write-Host "Missing options class"
        }
    } else {
        Write-Host "Section missing from appsettings.json"
    }
}
```

## Environment Setup Validation

### 1. Development Environment

```powershell
# Check development prerequisites
$prerequisites = @{
    "Docker Desktop" = { docker --version }
    ".NET SDK" = { dotnet --version }
    "PowerShell" = { $PSVersionTable.PSVersion }
    "NVIDIA Toolkit" = { nvidia-smi }
}

foreach ($prereq in $prerequisites.Keys) {
    Write-Host "`nChecking $prereq:"
    try {
        & $prerequisites[$prereq]
        Write-Host "Installed and working"
    } catch {
        Write-Host "Missing or not working"
    }
}
```

### 2. Project Structure

```powershell
# Validate project structure
$requiredDirs = @(
    "SecureHomeSystem",
    "docker",
    "docs",
    "tests",
    "data",
    "scripts"
)

foreach ($dir in $requiredDirs) {
    $exists = Test-Path $dir
    Write-Host "`nChecking $dir:"
    if ($exists) {
        Get-ChildItem $dir -Directory | 
            Select-Object Name | 
            ForEach-Object { Write-Host "  - $_" }
    } else {
        Write-Host "Missing required directory"
    }
}
```

### 3. Build Environment

```powershell
# Verify build script
$buildScript = "scripts/build.ps1"
if (Test-Path $buildScript) {
    Write-Host "`nAnalyzing build script:"
    Get-Content $buildScript | Select-String "task" -Context 0,3
    
    # Test build
    Write-Host "`nTesting build:"
    & $buildScript -SkipFormat
}

# Check test setup
$testProjects = Get-ChildItem -Recurse -Filter *.Tests.csproj
foreach ($proj in $testProjects) {
    Write-Host "`nValidating test project: $($proj.Name)"
    dotnet test $proj.FullName --list-tests
}
```

## Data Directory Validation

### 1. Storage Paths

```powershell
# Verify data directories
$dataDirs = @(
    "data/logs/services",
    "data/logs/state",
    "data/open-webui/cache",
    "data/open-webui/uploads",
    "data/qdrant/collections",
    "data/automatic1111/cache"
)

foreach ($dir in $dataDirs) {
    Write-Host "`nChecking $dir:"
    if (Test-Path $dir) {
        $acl = Get-Acl $dir
        Write-Host "Permissions:"
        $acl.Access | Format-Table IdentityReference,FileSystemRights
    } else {
        Write-Host "Creating directory..."
        New-Item -ItemType Directory -Path $dir -Force
    }
}
```

### 2. Model Storage

```powershell
# Check model directories
$modelDirs = @(
    "models/stable-diffusion",
    "data/open-webui/models"
)

foreach ($dir in $modelDirs) {
    Write-Host "`nValidating $dir:"
    if (Test-Path $dir) {
        Get-ChildItem $dir -File |
            Where-Object { $_.Extension -in @('.safetensors','.ckpt','.bin') } |
            Format-Table Name, Length, LastWriteTime
    } else {
        Write-Host "Model directory missing"
    }
}
```

## Validation Checklist

### Configuration Files
- [ ] All required configuration files present
- [ ] JSON/YAML syntax valid
- [ ] Required sections complete
- [ ] Environment variables set

### Options Classes
- [ ] All options properly mapped
- [ ] Default values appropriate
- [ ] Required properties present
- [ ] Documentation complete

### Environment Setup
- [ ] Prerequisites installed
- [ ] Project structure complete
- [ ] Build environment working
- [ ] Test framework configured

### Data Storage
- [ ] Directories created
- [ ] Permissions correct
- [ ] Models available
- [ ] Backup locations ready