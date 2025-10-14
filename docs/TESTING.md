# SecureHomeSystem Testing Guide

This document summarises the automated testing layers and how to run them locally.

## Prerequisites

- .NET SDK 9.0 or later
- Docker CLI (only required for smoke tests)

## Test Categories

| Category      | Description                                                                 | Command                                                            |
|---------------|-----------------------------------------------------------------------------|--------------------------------------------------------------------|
| `Unit`        | Fast tests that mock external processes (e.g. Docker detection).            | `dotnet test ShS.slnx --configuration Release --filter "Category=Unit"` |
| `Integration` | File-system heavy tests that exercise log cursor persistence and rotation.  | `dotnet test ShS.slnx --configuration Release --filter "Category=Integration"` |
| `Smoke`       | End-to-end Docker compose verification (opt-in, not part of default suite). | `pwsh ./scripts/smoke.ps1`                                         |

## Standard Test Run

The repository ships with `scripts/build.ps1`, which restores, formats, builds and tests the solution:

```powershell
pwsh ./scripts/build.ps1
```

You can skip formatting (useful while iterating) with:

```powershell
pwsh ./scripts/build.ps1 -SkipFormat
```

## Smoke Tests

Smoke tests spin up the stack using Docker Compose and then run only the tests tagged with `Category=Smoke`. They are optional because many CI environments do not provide Docker. To execute them manually:

```powershell
pwsh ./scripts/smoke.ps1
```

Pass `-SkipTeardown` to inspect containers after the run.

## Continuous Integration

The GitHub Actions workflow (`.github/workflows/ci.yml`) executes the build script on Windows and Linux. Use the “Run workflow” button in GitHub and set `run_smoke` to `true` to include the Docker-based smoke job.
