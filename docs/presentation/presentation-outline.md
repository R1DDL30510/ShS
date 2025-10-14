# SecureHomeSystem Presentation Outline

Use this outline to produce a polished slide deck (≈15 slides) for stakeholders. Each slide lists narrative points, data sources, and a screenshot placeholder with guidance on what to capture from your environment.

> 💼 **Release-Day Link:** Pair this outline with `docs/operations/release-day-playbook.md` so talking points, demo steps, and
> revision flags stay synchronised between the slide deck and live presentation.

## Slide 1 – Title & Vision
- Present the solution name: "SecureHomeSystem – AI-Assisted Home Orchestration".
- Tagline: "Coordinating LLM and image workloads for home automation".
- Include presenter name, date, and organization.
- **Screenshot placeholder:** `[[Screenshot: Repository banner or logo concept]]`  
  - Capture a clean hero graphic (repository splash image or Compose stack mascot).  
  - If no artwork exists, insert a temporary gradient background and plan to replace it later.

## Slide 2 – Agenda
- List 4–5 sections: Overview, Architecture, Components, Operations, Roadmap.
- Mention demo checkpoints (OpenWebUI, Stable Diffusion).
- **Screenshot placeholder:** `[[Screenshot: Table of contents layout]]`  
  - Export the slide outline once finalized to show flow; replace with stylized agenda graphic at design time.

## Slide 3 – Problem Statement & Goals
- Explain the orchestration challenge (multiple AI services, GPU contention, observability).
- Identify target audience (home lab operators, AI tinkerers).
- Summarize goals: unify monitoring, simplify GPU policy communication, streamline deployments.
- **Screenshot placeholder:** `[[Screenshot: Whiteboard photo or diagram of current pain points]]`  
  - Photograph an annotated sketch or create a quick digital whiteboard summarizing present challenges.

## Slide 4 – Solution Overview
- Describe the .NET 9 worker, Docker Compose stack, and generative AI tooling.
- Map services to roles (worker = orchestrator, OpenWebUI = LLM front-end, Qdrant = vector store, AUTOMATIC1111 = image generation).
- **Screenshot placeholder:** `[[Screenshot: README overview section]]`  
  - Open `README.md` in the IDE preview, zoom in on the Overview table, and capture it cleanly.

## Slide 5 – High-Level Architecture
- Present the interaction between worker, Docker daemon, containers, and host GPU.
- Mention label-based detection (`shs.role`) and environment configuration.
- **Screenshot placeholder:** `[[Screenshot: Architecture diagram]]`  
  - Build or export a diagram showing: worker container ↔ Docker socket ↔ OpenWebUI/Qdrant/AUTOMATIC1111.  
  - Tools: draw.io, Visio, or PowerPoint SmartArt.

## Slide 6 – Compose Stack Topology
- Highlight Compose profiles (`worker`, `diffusion`) and optional components.
- Summarize volume mounts and GPU reservation settings.
- Point to `docker/compose.yaml` for authoritative configuration.
- **Screenshot placeholder:** `[[Screenshot: Docker Desktop containers view]]`  
  - Launch the stack, open Docker Desktop, filter by project name `shs-stack`, and capture running services.

## Slide 7 - SecureHomeSystem Worker Deep Dive
- Outline worker responsibilities: load config, detect containers, log status every 30 seconds.
- Call out `Worker.cs` detection loop and `IDockerServiceDetector` abstraction.
- Highlight the new `/health` and `/live` endpoints surfaced by `Program.cs` for monitoring integrations.
- **Screenshot placeholder:** `[[Screenshot: Worker logs in terminal]]`  
  - Run `docker compose -f docker/compose.yaml logs -f shs-worker` and capture a segment showing detections.

## Slide 8 – OpenWebUI & Qdrant
- State how OpenWebUI provides LLM UX and uses Qdrant for RAG.
- Reference environment variables from `docker/.env` (`OLLAMA_BASE_URL`, `WEBUI_PORT`, etc.).
- Note default authentication (`OPENWEBUI_AUTH=false`) and data directories.
- **Screenshot placeholder:** `[[Screenshot: OpenWebUI landing page]]`  
  - Access `http://localhost:3000`, capture the main dashboard once models load.

## Slide 9 – Stable Diffusion (AUTOMATIC1111)
- Explain optional diffusion profile and GPU reservations.
- Mention startup patch script (`docker/automatic1111_patch.py`) and VRAM-saving flags.
- Highlight prerequisites: NVIDIA drivers, `nvidia-smi`.
- **Screenshot placeholder:** `[[Screenshot: AUTOMATIC1111 txt2img tab]]`  
  - With the diffusion profile active, open `http://localhost:7860` and capture the UI after loading.

## Slide 10 – Configuration Management
- Summarize key settings from `SecureHomeSystem/appsettings.json` (Docker detection, service endpoints, resource scheduler).
- Point to `docs/reference/configuration.md` for full key list.
- Call out environment overrides via `docker/.env`.
- **Screenshot placeholder:** `[[Screenshot: appsettings.json in IDE]]`  
  - Highlight relevant sections (Docker, Services, ResourceScheduler) with IDE syntax highlighting.

## Slide 11 - Operations & Runbook
- Reference procedures in `docs/operations/runbook.md` (status checks, restarts, troubleshooting).
- Mention GPU policy coordination (`docs/operations/gpu-policy.md`).
- Call out zentrale Log-Ablage (`/logs/worker`, `/logs/services`, Cursor unter `/logs/state`) mit Hinweis auf die integrierte Rotation (`LogCollector:Rotation`) und wie Operatoren darauf zugreifen.
- Emphasize manual enforcement of GPU guardrails (revision flag).
- **Screenshot placeholder:** `[[Screenshot: Runbook markdown rendered]]`  
  - Use the IDE markdown preview or a documentation site rendering to illustrate operator guidance.

## Slide 12 – Deployment Workflow
- Steps for local compose bring-up, profile-specific start, and log validation.
- Include quick start commands from `docs/setup/docker-compose.md`.
- Note `.env` customization for host paths and ports.
- **Screenshot placeholder:** `[[Screenshot: Terminal running docker compose up]]`  
  - Show successful startup output with services entering `healthy` or `running` state.

## Slide 13 – Demo Plan & Checkpoints
- Outline demo flow:
  1. Launch worker profile and confirm OpenWebUI/Qdrant.
  2. Enable diffusion profile and validate Stable Diffusion.
  3. Showcase worker logs detecting services.
- Tie checkpoints to commands listed in `docs/setup/docker-compose.md`.
- **Screenshot placeholder:** `[[Screenshot: Checklist spreadsheet or planner]]`  
  - Capture the project management tool or checklist you will use during the live demo.

## Slide 14 – Risks, Flags & Follow-Up Actions
- Present outstanding items to verify before publication:
  - Keine automatisierten Tests (`README.md` - Abschnitt **Revision Flags**).
  - GPU-Policy-Automatisierung steht noch aus (`README.md` - Abschnitt **Revision Flags**, `docs/operations/gpu-policy.md`).
  - Zentrale Logs liegen als Dateien vor; es fehlt weiterhin eine Query-/Alerting-Schicht (`README.md` - Abschnitt **Logging & Observability**).
  - GPU-Dokumentation für Limits aktualisieren (`docs/setup/docker-compose.md` - Abschnitt **GPU Limits**).
- Suggest owner assignment and timelines to resolve each flag.
- **Screenshot placeholder:** `[[Screenshot: Issue tracker board highlighting follow-ups]]`  
  - Use your preferred tracker (GitHub Projects, Jira) to display tasks mapped to each flag.

## Slide 15 – Roadmap & Next Steps
- Feature roadmap: automated scheduling, health endpoints, test harness, telemetry dashboards.
- Invite collaborators and outline contribution process (`README.md` Support section).
- Provide call to action (e.g., "Pilot in home lab", "Contribute test suites").
- **Screenshot placeholder:** `[[Screenshot: Roadmap timeline graphic]]`  
  - Generate a timeline diagram (PowerPoint SmartArt or external tool) showing upcoming milestones.

## Appendix – Reference Material
- Link to key docs: `README.md`, `docs/reference/configuration.md`, `docs/operations/runbook.md`, `docs/operations/gpu-policy.md`, `docs/setup/docker-compose.md`.
- Include environment summary (`docker/.env`), Compose command cheatsheet, and repository structure.
- **Screenshot placeholder:** `[[Screenshot: Repository tree in IDE]]`  
  - Capture the IDE explorer showing top-level directories and important files.

---

### Presentation Production Tips
- Reuse the markdown content directly in slide notes to keep technical accuracy.
- Maintain consistent styling across screenshots (same resolution, dark/light mode).
- Mask sensitive hostnames or IDs before sharing externally.
- Keep revision flags visible until remediations land; update slides as soon as the codebase changes.
