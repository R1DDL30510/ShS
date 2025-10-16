# Resources and Environment Overview

## Operating System

| Attribute | Value |
|-----------|-------|
| Kernel | `Linux` |
| Distribution | `Ubuntu 24.04.3 LTS (Noble Numbat)` |
| Kernel Release | `6.6.87.2-microsoft-standard-WSL2` |
| Architecture | `x86_64` |

## Shell & Tool Availability

* Shell: `bash`
* Command tools used during interaction:
  * `ls` – directory listing
  * `cat` – file contents
  * `echo` – text output / redirection
  * `mkdir` / `rm` – directory/file manipulation
  * `dpkg` & `apt` – package information (Debian/Ubuntu)

## Installed Packages (excerpt)

The following table lists a few of the packages present in the
environment to illustrate how many packages are installed; running a
full `dpkg -l` would output over 1000 entries.

| Name | Version | Description |
|------|---------|------------|
| `adduser` | `3.137ubuntu1` | add and remove users and groups |
| `apparmor` | `4.0.1really4.0.1-0ubuntu0.24.04.4` | user‑space parser utility for AppArmor |
| `bash` | `5.2.12(1)-release` | GNU Bourne Again shell |
| `coreutils` | `8.32` | basic file, shell, and text utilities |
| `dpkg` | `1.22.5ubuntu3.12` | package manager |
| `grep` | `3.11` | search text with regular expressions |
| `sed` | `4.9` | stream editor |
| `wget` | `1.21.3` | network download utility |

## Available Commands Overview

The next section re‑organises the command capabilities table into a
more readable format, grouping commands by functional area (e.g.
file‑system, networking, package‑management, etc.).

---

### File‑system Operations

| Command | Description |
|---------|-------------|
| `ls` | List files and directories |
| `cat` | Read file contents |
| `echo` | Output text, used for redirection |
| `touch` | Create an empty file |
| `mkdir` | Make a new directory |
| `mv` | Move or rename files |
| `rm` | Remove files/directories |

### System Information

| Command | Description |
|---------|-------------|
| `uname -a` | Kernel and OS info |
| `cat /etc/os-release` | Distribution details |

### Package Management

| Command | Description |
|---------|-------------|
| `dpkg -l` | List installed packages |
| `apt-cache policy` | Show package repository info |

### Networking (restricted)

The sandbox environment disables outbound network traffic.  This
restriction means commands such as `ping` or `curl` will fail with a
permission or timeout error.  The table below lists typical network
commands and their expected behaviour in this sandbox.

| Command | Description |
|---------|-------------|
| `ping`  | ICMP echo request – blocked, returns an error |

## Interaction Guidelines

* **Approve privileged commands** – When a command may require elevated
  privileges or outside‑sandbox access, the harness will request user
  approval. Always check the output for permission errors.
* **Avoid network‑intensive commands** – The network sandbox is
  *restricted*, so commands that open sockets (e.g.
  `nc`, `curl`, `wget` with remote URLs) will typically be denied.
* **Log outputs** – For reproducibility, capture command outputs and
  record them in the relevant documentation files.

---

This document now provides a single, clear overview of the
environment, installed packages, and how to interact with the agent.
