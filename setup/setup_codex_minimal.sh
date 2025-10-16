#!/usr/bin/env bash
# Minimal Codex environment setup
#
# This script performs a tiny bootstrap that is guaranteed to run in a
# sandboxed environment.  It can be expanded in the future to install
# packages, set environment variables, or create symlinks.

set -euo pipefail

usage() {
    cat <<'EOF'
Usage: $0 [--help]

Options:
  --help,-h    Show this help message and exit.
EOF
    exit 0
}

first_arg=${1:-}
if [[ "$first_arg" == "--help" || "$first_arg" == "-h" ]]; then
    usage
fi

echo "Setting up minimal Codex environment..."
# Example placeholder – install one small dependency if needed.  In the
# Codex sandbox the network is **restricted**, so this is a no‑op.
# Uncomment the line below to install a package when network access is
# available.
# apt-get install -y curl
echo "Setup complete."
