# Repository Guidelines

This document serves as a quick, actionable reference for contributing to the **vault** project. Follow it to keep the codebase clean, consistent, and easy to maintain.

## Project Structure & Module Organization

The repository is intentionally small:

```
├── AGENTdocs/      # Markdown resources for agents
├── setup/          # Bash install scripts (e.g. setup_codex_minimal.sh)
└── README.md       # Project overview
```

All source code lives in `setup/`. When adding new scripts, place them there or in a sub‑folder, respecting the existing naming convention.

## Build, Test, and Development Commands

| Command | What it does |
|---------|--------------|
| `chmod +x setup/setup_codex_minimal.sh` | Make the install script executable |
| `bash setup/setup_codex_minimal.sh` | Run the minimal Codex setup (configures environment) |
| `./setup/setup_codex_minimal.sh --help` | Show script usage |
|
**Future** – a top‑level `Makefile` may expose `make build`, `make test`, and `make clean` once additional components are added.

## Coding Style & Naming Conventions

- **Indentation**: 2 spaces (no tabs). |
- **File names**: snake_case (e.g. `setup_codex_minimal.sh`). |
- **Shell scripts**: Bash; start with `#!/usr/bin/env bash`. |
- **Linting**: Run `shellcheck ./setup/*.sh` before committing. |

Follow these rules to keep the code consistent and readable.

## Testing Guidelines

The project currently has no formal tests. When they are added:

- Test files live in `tests/` and match `test_*.sh`. |
- Run the suite with `bash setup/run_tests.sh` or similar. |
- Naming: `test_<feature>.sh`. |

Ensure tests cover new scripts and that they pass locally.

## Commit & Pull Request Guidelines

- **Commit messages**: Use [Conventional Commits](https://www.conventionalcommits.org/). Example: `feat: add install script helper`. |
- **Pull requests**: Must have a clear title, description, and reference any related issue (`Fixes #42`). |
- Include screenshots or demo shell sessions if the change is visual or demonstrates behavior. |
- All PRs must pass CI status checks; the CI runs `shellcheck` on Bash scripts. |

## Security & Configuration Tips

- Never commit secrets (API keys, passwords). |
- If the setup script needs environment variables, add a `.env.example` template. |
- Keep the install script idempotent – running it twice should yield the same result. |

---

For additional context, see `README.md` and the `setup/` directory.
