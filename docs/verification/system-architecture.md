# ShS System Plan

## Architecture

### Core Services
- **Worker (`Worker.cs`)**: Runs the orchestration loop at 30-second intervals, logging Ollama, OpenWebUI, and StableDiffusion endpoints during startup before entering the detection lifecycle. Leverages Dependency Injection, the Background Service pattern, and `ServiceEndpointsOptions` to maintain configurable URLs.
- **Docker Service Detection (`DockerServiceDetector.cs`)**: Executes `docker ps` via the `IProcessRunner` abstraction, parses container metadata, and maps services through label selectors defined in `DockerOptions`, employing the Strategy and Command patterns for extensibility.
- **Container Log Collector (`ContainerLogCollector.cs`)**: Performs cursor-based log harvesting in JSON Lines format with timestamp deduplication, configurable rotation, and dependency on `IDockerServiceDetector`, `IProcessRunner`, and log configuration options.

### Data and Control Flows
- **Service Detection Sequence**: Worker triggers `DetectAsync`, `DockerServiceDetector` issues `docker ps` and maps results to logical services, then the Worker records health status.
- **Log Collection Sequence**: For each detected service, the log collector requests `docker logs --since`, parses timestamps, writes structured logs to disk, and updates per-service cursors to avoid duplication.

### Configuration Model
- **ServiceEndpointsOptions** maintain base URLs for Ollama (`http://localhost:11434`), OpenWebUI (`http://localhost:3000`), and StableDiffusion (`http://localhost:7860`).
- **DockerOptions** hold label selectors such as `shs.role=webui`, `shs.role=vector-db`, and `shs.role=diffusion` to isolate containers.
- **LogCollectorOptions** and **LogStorageOptions** define collection cadence, rotation thresholds, storage roots (`/logs/services/` for JSONL output, `/logs/state/` for cursors), and retention rules.

### Resource and Process Management
- Cursor-based log collection and 30-second detection intervals balance responsiveness with resource efficiency.
- Size and age-based log rotation prevent unbounded growth while enabling historical retention.
- `IProcessRunner` provides cancellation-aware Docker CLI execution with retry and error-handling hooks for graceful shutdowns.

### Operational Interfaces
- HTTP health checks expose service status, and structured logging supports observability alongside container status tracking.
- Error diagnostics capture process exit codes and validate container state to accelerate troubleshooting.

## Methodology

### Assurance Practices
- **Unit Testing**: Mock `IProcessRunner` to validate detection logic, configuration binding, and exception handling paths.
- **Integration Testing**: Compose Worker, Detector, and Collector instances to execute end-to-end detection and log collection workflows, asserting expected log entries and state transitions.
- **Configuration Verification**: Continuously validate runtime options against deployment baselines to prevent misconfiguration drift.

### Operational Cadence
- Maintain iterative reviews of detection intervals, label selectors, and rotation thresholds to confirm alignment with workload patterns.
- Document and replay troubleshooting runbooks to institutionalize incident response steps and shorten recovery time.

## Security Model

### Isolation Controls
- Apply label-based service identification to segment workloads while restricting Docker daemon access to read-only operations mediated through `IProcessRunner`.
- Enforce filesystem permissions on log directories, limiting mutation to the collection service account.

### Data Protection Measures
- Timestamped cursors eliminate duplicate ingestion, reducing risk of replaying sanitized inputs.
- Structured logging pipelines sanitize inputs before persistence and maintain least-privilege access to stored artifacts.

### Operational Guardrails
- Monitor health endpoints for anomalous behavior, integrate container status tracking into alerting, and verify process exit codes before reinitialization.
- Audit configuration changes, especially label selectors and endpoint URLs, to preserve trusted service boundaries.

## Coverage and Planning Overview

| Team | Current Coverage Summary | Score (0–5) | Next Planning Actions |
| --- | --- | --- | --- |
| Environment | Baseline configuration captured through `ServiceEndpointsOptions`, `DockerOptions`, and log storage paths. | 3 | Harden environment baselines with configuration drift detection and document runtime overrides. |
| Network | Endpoint inventory for Ollama, OpenWebUI, and StableDiffusion with label-based container routing. | 2 | Define ingress/egress rules, verify network isolation between workloads, and model service discovery latency budgets. |
| UI | Health endpoints provide status visibility; no dedicated UI workflow captured. | 1 | Collaborate with WebUI owners to extend monitoring dashboards with log summaries and detection metrics. |
| Stability & Monitoring | Structured logs, HTTP health checks, and process exit diagnostics documented. | 3 | Integrate alert thresholds for detection failures, automate rotation validation, and schedule load resilience reviews. |
| Security | Read-only Docker interactions, filesystem permissions, and sanitization practices outlined. | 4 | Formalize access review cadence, expand secret handling guidance, and validate compliance against organizational controls. |

### Change Validation Summary
- Retained all original technical intents by restating Worker cadence, detection mechanics, log collection behavior, and configuration structures while organizing them under Architecture, Methodology, and Security Model.
- Clarified operational and security guardrails to align with best-practice documentation without altering existing design assumptions.
- Added cross-team planning table to surface coverage status, scores, and actionable next steps for environment, network, UI, stability/monitoring, and security teams.

