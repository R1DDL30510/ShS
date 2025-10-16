"""Simple Makefile for the Codex Vault repository.

The provided target installs the `codex` CLI by running the minimal
bootstrap script.  It also adds the local binary path to your shell
profile if needed.
"""

.PHONY: install

install:
	@echo "=== Installing Codex CLI via bootstrap script ==="
	chmod +x setup/setup_codex_minimal.sh
	./setup/setup_codex_minimal.sh
	@echo "=== Install complete. Ensure ~/.local/bin is in PATH ==="
