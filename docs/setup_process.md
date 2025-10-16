# Codex Sandbox Setup Documentation

This document records the steps used to create the local Codex
environment that lives inside a WSL2 Ubuntu distribution on a
Windows‑11 host.  The setup is intentionally minimal – it keeps the
Ollama model locally, runs the Codex CLI, and stores the root project
(the Obsidian vault) under the current working directory.

## 1. Install WSL2 + Ubuntu

1. Open **PowerShell** as Administrator.
2. Enable the WSL feature:
   ```powershell
   wsl --install
   ```
3. Restart the machine.
4. Open the Microsoft Store and install **Ubuntu 22.04 LTS**.
5. Launch Ubuntu, create a user, and ensure the distribution is set to
   use the **WSL 2** kernel:
   ```bash
   wsl --set-default-version 2
   ```

## 2. Install Ollama (the local LLM host)

```bash
curl -fsSL https://ollama.com/install.sh | sh
```

Start the Ollama daemon:

```bash
ollama serve
```

If you prefer Docker, you can pull the `ollama/ollama` image and
run it locally.

## 3. Pull the custom model

The user has a custom `gpt-oss:20b` model.  It was created with a
`Modelfile` that tweaks temperature, token limits, and a base prompt.

```bash
ollama pull gpt-oss:20b
```

To verify the model is available:

```bash
ollama list
```

## 4. Install the Codex CLI inside the Ubuntu distro

```bash
# Install from pip (or the provided installer script)
python3 -m pip install --user codex
```

Add the binary path to your `$PATH` if it isn’t already:

```bash
export PATH="$HOME/.local/bin:$PATH"
```

## 5. Configure the Codex CLI

Create `~/.config/codex/config.toml` (or a local `config.toml` in
the project root).  Example contents:

```toml
[server]
endpoint = "http://127.0.0.1:11434"
model = "gpt-oss:20b"

[options]
verbose = true
```

The endpoint points to the local Ollama daemon.  The `model` field
specifies the custom model that should be used for all Codex requests.

### Validation

```bash
codex info
codex config status
```

Both commands should report the correct endpoint and model.

## 6. Sync the Obsidian vault / repository

The root of the repository (`/mnt/c/Users/MvP/source/vault`) is an
Obsidian vault that holds notes, documentation and the codebase.  The
vault can be edited inside Windows (via any editor) or inside WSL.

Synchronisation is handled by Windows' built‑in file sharing: the
Windows folder is mounted by WSL under `/mnt/c/...`.  No additional
sync steps are required.

## 7. Test the setup

Launch the Codex interactive shell:

```bash
codex repl
```

Ask a simple question to verify that the model is answering. For
example:

```text
What is the capital of France?
```

If a coherent answer is returned (e.g., *Paris*), the setup is
working.

---

### Known Issues / Things to Double‑Check

- Ensure the WSL distribution is actually running **WSL 2**.  The
  Codex CLI expects the network stack to be working, and some older
  WSL1 builds have intermittent network issues.
- If the `codex config status` command fails, double‑check that the
  `config.toml` file is located in the correct directory and that
  `codex` can read it.
- The custom `gpt-oss:20b` model requires ~20 GB of storage.  If the
  disk is low on space, the `ollama pull` command will fail.
- The example Modelfile used to generate the custom model is not
  shown here.  It typically contains a `FROM` line pointing to a
  base model and a series of `SYSTEM`/`PROMPT` directives.

---

> **Tip:** Put this markdown file in your Obsidian vault and keep it
> updated as you tweak the installation – it becomes a living
> reference for future experiments.

