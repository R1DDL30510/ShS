# GPU-Richtlinie

Dieses Repository hält gewünschte GPU-Auslastungsziele in der Konfiguration und in den Compose-Einstellungen fest. Automatisierung für dynamisches Scheduling wurde noch nicht umgesetzt, daher setzen Operator:innen diese Limits derzeit manuell durch.

## Konfigurationsziele
- `ResourceScheduler:GpuUtilisationThreshold = 0.5`: bevorzugte Obergrenze für anhaltende GPU-Auslastung.
- `ResourceScheduler:GpuMemoryThreshold = 0.8`: gewünschtes VRAM-Limit pro Gerät.
- `ResourceScheduler:PollIntervalSeconds = 5` / `QueueBackoffSeconds = 30`: Platzhalter, die die geplante Abtast- und Retry-Frequenz dokumentieren (Entwicklungs-Overrides reduzieren auf `3` bzw. `10`).

## Container-Parameter
- Ollama: `OLLAMA_MAX_GPU_MEMORY` ist in `docker/compose.yaml` standardmäßig auf `0.8` gesetzt und entspricht dem VRAM-Ziel.
- Stable Diffusion: Die Compose-Startparameter enthalten `--medvram` und `--opt-sdp-attention`, um den Speicherverbrauch zu steuern; zusätzliche Argumente können über die Umgebungsvariable `AUTOMATIC1111_ARGS` übergeben werden.
- GPU-Zugriff wird für Stable Diffusion über das Profil `diffusion` aktiviert (`deploy.resources.reservations.devices`).

## Operative Leitplanken
- Beobachten Sie die Auslastung in Echtzeit mit `nvidia-smi` (WSL2 oder Host-Shell) oder über herstellerspezifische Dashboards.
- Pausieren Sie rechenintensive Jobs, indem Sie den Stable-Diffusion-Container stoppen: `docker compose --profile diffusion stop automatic1111`.
- Nehmen Sie Workloads wieder auf, sobald die Auslastung unter die konfigurierten Zielwerte fällt, und starten Sie Container mit `docker compose up -d` neu.

## Zukünftige Erweiterungen
- Automatisierte Durchsetzung auf Basis der hinterlegten Schwellen implementieren.
- Telemetrie und Alarmierung (Metriken/Ereignisse) bereitstellen, sobald Scheduling-Logik vorhanden ist.

> ⚠️ **Revisionshinweis:** Aktualisieren Sie diese Richtlinie, sobald Scheduling-Automatisierung oder Telemetrie-Features ausgeliefert werden.
