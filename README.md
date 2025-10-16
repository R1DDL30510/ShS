# Codex Vault

This repository provides a minimal, reproducible environment for
experimenting with the **Codex** tools.  The main entry point is the
`setup/` directory which contains a compact Bash script that bootstraps
the environment and installs the `codex` CLI.

## Repository Structure

```
├── AGENTdocs/      # Markdown resources explaining agent policies
├── setup/          # Installation scripts and readme
├── docs/           # Runtime dependencies reference
└── README.md       # Top‑level overview (this file)
```

## Quick‑Start

```bash
# Make the install script executable
chmod +x setup/setup_codex_minimal.sh

# Run the bootstrap – this will install the `codex` package via pip
./setup/setup_codex_minimal.sh
```

> **Prerequisite**: Ensure Python 3 and `pip` are available in the
> environment.  The bootstrap script uses `python3 -m pip` to install
> the `codex` package.

After the bootstrap you can launch the Codex REPL with `codex repl` and
start experimenting.  No additional build artifacts are required.
