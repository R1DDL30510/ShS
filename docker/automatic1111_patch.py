#!/usr/bin/env python3
"""
Kompatibilitätspatch für ältere AUTOMATIC1111-Builds anwenden.
Das verwendete Upstream-Image liefert eine models.py, in der das generierte
OptionsModel `sd_model_checkpoint` als Optional[NoneType] behandelt. Sobald ein
Checkpoint gewählt ist, schlägt dadurch die Antwort von /sdapi/v1/options fehl.

Dieses Skript überschreibt den relevanten Block, damit das Feld als Optional[str]
typisiert ist und ein nicht-leerer Standardwert gesetzt wird. Der Ablauf ist
idempotent und kann gefahrlos mehrfach ausgeführt werden.
"""
from __future__ import annotations

from pathlib import Path

MODEL_PATH = Path("/stable-diffusion-webui/modules/api/models.py")


def apply_patch() -> bool:
    text = MODEL_PATH.read_text()
    sentinel = 'metadata.default = metadata.default or ""'
    if sentinel in text:
        return False

    target = (
        "    if (metadata is not None):\n"
        "        fields.update({key: (Optional[optType], Field(\n"
        "            default=metadata.default ,description=metadata.label))})"
    )

    replacement = (
        "    if key == \"sd_model_checkpoint\":\n"
        "        optType = str\n"
        "        metadata.default = metadata.default or \"\"\n"
        "        value = value or metadata.default\n\n"
        "    if (metadata is not None):\n"
        "        fields.update({key: (Optional[optType], Field(\n"
        "            default=metadata.default ,description=metadata.label))})"
    )

    if target not in text:
        raise RuntimeError("Erwartete Struktur in models.py nicht gefunden – Patch wird abgebrochen.")

    MODEL_PATH.write_text(text.replace(target, replacement, 1))
    return True


def main() -> None:
    changed = apply_patch()
    if changed:
        print("Kompatibilitätspatch für AUTOMATIC1111-OptionsModel angewendet.")
    else:
        print("Patch bereits vorhanden; keine Änderungen vorgenommen.")


if __name__ == "__main__":
    main()
