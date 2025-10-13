#!/usr/bin/env python3
"""
Apply compatibility patch for legacy AUTOMATIC1111 builds.
The upstream image we consume ships a models.py where the generated
OptionsModel treats sd_model_checkpoint as Optional[NoneType], which breaks
the /sdapi/v1/options response once a checkpoint is selected.

This script rewrites the relevant block so the field is typed as Optional[str]
and ensures a non-null default. It is idempotent and safe to run multiple times.
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
        raise RuntimeError("Expected models.py structure not found; aborting patch.")

    MODEL_PATH.write_text(text.replace(target, replacement, 1))
    return True


def main() -> None:
    changed = apply_patch()
    if changed:
        print("Applied automatic1111 OptionsModel compatibility patch.")
    else:
        print("Patch already present; no changes made.")


if __name__ == "__main__":
    main()
