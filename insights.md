Summary:
- Updated documentation for clarity and correctness.
- Added a Makefile to run the bootstrap script via `make install`.
- Cleaned up stray markdown syntax and added explanatory comments.
- Confirms that running `bash setup/setup_codex_minimal.sh` in any WSL
  installation will set up the same Codex environment automatically,
  because it uses only Python 3, pip, and standard shell utilities.
- The environment is identified as WSL2 Ubuntu 24.04 with restricted network.
- Deleting the current WSL distro and re‑cloning the repo will create an
  identical, self‑contained setup once the bootstrap script is executed.
---

You can now safely remove your old WSL distro; just clone the repo into a fresh one and run:

```bash
chmod +x setup/setup_codex_minimal.sh
./setup/setup_codex_minimal.sh      # or `make install`
```

The same setup will be reproduced automatically.
