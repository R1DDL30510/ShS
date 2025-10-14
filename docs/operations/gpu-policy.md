# GPU-Richtlinie

Das Repository hält gewünschte GPU-Auslastungsziele in der Konfiguration und in Compose-Einstellungen fest. Eine Automatisierung für dynamisches Scheduling existiert noch nicht, daher setzen Operator:innen die Limits aktuell manuell durch.

## Konfigurationsziele
- `ResourceScheduler:GpuUtilisationThreshold = 0.5`: bevorzugte Obergrenze für anhaltende GPU-Auslastung.
- `ResourceScheduler:GpuMemoryThreshold = 0.8`: angestrebtes VRAM-Limit pro Gerät.
- `ResourceScheduler:PollIntervalSeconds = 5` / `QueueBackoffSeconds = 30`: Platzhalter, die die gewünschte Abtast- und Retry-Frequenz dokumentieren (Entwicklungs-Overrides reduzieren auf `3` bzw. `10`).

## Container-Parameter
- Ollama: `OLLAMA_MAX_GPU_MEMORY` ist in `docker/compose.yaml` standardmäßig auf `0.8` gesetzt und entspricht damit dem VRAM-Ziel.
- Stable Diffusion: Die Compose-Commandline enthält `--medvram` und `--opt-sdp-attention`, um den Speicherverbrauch zu steuern; zusätzliche Argumente können über die Umgebungsvariable `AUTOMATIC1111_ARGS` gesetzt werden.
- GPU-Zugriff wird für Stable Diffusion über das Profil `diffusion` aktiviert (`deploy.resources.reservations.devices`).

## Operative Leitlinien
- Echtzeit-Auslastung mit `nvidia-smi` (WSL2 oder Host-Shell) oder Hersteller-Dashboards beobachten.
- Rechenintensive Jobs pausieren, indem der Stable-Diffusion-Container gestoppt wird: `docker compose --profile diffusion stop automatic1111`.
- Workloads wieder aufnehmen, sobald die Auslastung unter die konfigurierten Ziele fällt, und Container mit `docker compose up -d` neu starten.

## Zukünftige Verbesserungen
- Automatisierte Durchsetzung auf Basis der hinterlegten Schwellen implementieren.
- Telemetrie und Alerting (Metriken/Ereignisse) bereitstellen, sobald Scheduling-Logik existiert.

> ⚠️ **Revisionsflag:** Aktualisiere diese Richtlinie, sobald Scheduling-Automatisierung oder Telemetrie-Funktionen geliefert werden.
