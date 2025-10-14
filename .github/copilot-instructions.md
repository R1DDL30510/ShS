# SecureHomeSystem AI Agent Instructions

## Content Map
```
SecureHomeSystem/
├── Configuration/       # Strongly-typed options classes
├── Infrastructure/     # Process runner abstraction
├── Models/            # Shared data structures
├── Services/          # Core services implementation
└── Worker.cs         # Main orchestration loop

docker/
├── compose.yaml      # Stack definition with profiles
└── automatic1111_patch.py  # Stable Diffusion fixes

docs/
├── reference/        # Configuration reference
├── setup/           # Environment setup guides
└── operations/      # Production policies
```

## Project Overview
SecureHomeSystem is a .NET 9 orchestration layer for an AI-assisted home automation stack. The system combines:
- A .NET worker service for Docker container orchestration
- OpenWebUI front-end for Ollama LLM integration (port 3000)
- Qdrant vector database for RAG operations (port 6334)
- Optional Stable Diffusion WebUI for image generation (port 7860)

## Key Architecture Patterns

### Worker Service Architecture
- Core orchestration in `SecureHomeSystem/Worker.cs` runs 30-second detection cycles
- Docker service detection via `IDockerServiceDetector` monitors container health
- Health endpoints exposed on configured port (default 5080) via minimal API
- Log collection for containers with `shs.role` labels when enabled
- Cursor-based log harvesting with configurable rotation policies

### Configuration Patterns
- Strongly-typed options pattern used throughout (`SecureHomeSystem/Configuration/`)
- Environment variables override appsettings.json via Docker Compose
- Service endpoints configured in `ServiceEndpointsOptions.cs`
- Docker access configured through `DockerOptions.cs`
- GPU policies defined in `ResourceSchedulerOptions.cs` (targeting 80% VRAM)

### Testing Patterns
- Unit tests mock Docker CLI with `FakeProcessRunner`
- Integration tests exercise log rotation and cursors
- Smoke tests verify full stack with `scripts/smoke.ps1`
- GitHub Actions workflow in `.github/workflows/ci.yml`

## Development Workflow

### Prerequisites
- Docker Desktop with WSL2 backend
- NVIDIA Container Toolkit (for GPU workloads)
- Ollama running on port 11434 (for LLM features)
- .NET SDK 9.0+

### Local Development
```powershell
# Build the worker
./scripts/build.ps1 [-SkipFormat]  # Format optional

# Start the stack (worker + OpenWebUI + Qdrant)
docker compose --profile worker up -d

# Start with Stable Diffusion
docker compose --profile worker --profile diffusion up -d

# View logs
docker compose logs -f
docker compose logs shs-worker  # Worker only
```

### Common Patterns
1. New service integration:
   - Add options class in `Configuration/`
   - Configure in `Program.cs`
   - Add detection in `DockerServiceDetector`
   - Update compose.yaml with `shs.role` label

2. Container health monitoring:
   - HTTP health checks defined per service
   - Worker: `http://localhost:5080/health`
   - OpenWebUI: `http://localhost:3000/api/system/info`
   - Qdrant: `http://localhost:6334/readyz`
   - StableDiffusion: Requires API call to `/sdapi/v1/txt2img`

3. GPU resource management:
   - Configure limits in `ResourceSchedulerOptions`
   - Set VRAM cap via `OLLAMA_MAX_GPU_MEMORY`
   - Control GPU selection with `AUTOMATIC1111_GPU_*` vars
   - Monitor with `nvidia-smi` in WSL

## Important Conventions
- Container roles tagged with `shs.role` labels
- Log directories auto-created under `/logs` mount
- 30-second detection interval optimized for demos
- Services defined in docker-compose use profile groups
- Health/liveness paths normalized with leading slash
- GPU policies target 50% utilization, 80% VRAM
- Structured logging with Serilog to files and console