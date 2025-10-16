# Repository Guidelines

This document provides a quick, actionable reference for working on **this**
repository.  Please read it before submitting code or documentation changes.

## Project Structure & Module Organization

```
├── AGENTdocs/      # Documentation and markdown resources
├── setup/          # Shell scripts and installer helpers
└── README.md      # High‑level project description
```

* Source code lives in `setup/` – it contains a single install script that
  prepares the minimal Codex environment.
* All additional documentation should be added to the `AGENTdocs` folder.

## Build, Test, and Development Commands

| Command | What it does |
|---------|--------------|
| `chmod +x setup/setup_codex_minimal.sh` | Make the install script executable |
| `bash setup/setup_codex_minimal.sh` | Run the minimal Codex setup (pre‑configure the environment) |
| `./setup/setup_codex_minimal.sh --help` | Show script usage |

If you add new build steps, expose them via `make <target>` in a top‑level
`Makefile`.

## Coding Style & Naming Conventions

* **Indentation:** 2 spaces per level (no tabs).
* **File names:** snake_case, e.g., `setup_codex_minimal.sh`.
* **Shell scripts:** use Bash; include `#!/usr/bin/env bash` shebang.
* **Linting:** run `shellcheck` on all `.sh` files.

All changes should pass `shellcheck ./setup/*.sh` before committing.

## Testing Guidelines

The repository has no formal unit tests yet.  In the future, we intend to use
`npx jest` for JavaScript tests and `pytest` for Python scripts.

* Test files live in `tests/` and are named `test_*.py` or `*.spec.js`.
* Run the full suite with `npm test` or `pytest -q` once added.

## Commit & Pull Request Guidelines

* **Commit messages:** follow [Conventional Commits](https://www.conventionalcommits.org/).  e.g.,
  ````
  feat: add install script helper
  ```
* **Pull requests:** must include a clear title, description, and link to any
  relevant issue (e.g., `Fixes #42`).  Add screenshots if the change is
  visual or a demo shell session.  All PRs must pass the CI status check.

## Security & Configuration Tips

* Never commit sensitive data (API keys, passwords) to the repo.
* Use a `.env.example` template if a project requires environment variables.
* When expanding the setup script, keep it idempotent—running it twice should
  produce the same outcome.

[See the project README](./setup/README.md) for additional setup instructions.
## Tool Validation
The `AGENTtool.md` document lists each available tool and a simple test case that
verifies it works in the current sandbox.  Refer to that file for quick
validation snippets and the current status of each tool.
