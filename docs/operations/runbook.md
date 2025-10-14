# Operations-Runbook

## Statusprüfungen
- `docker compose -f docker/compose.yaml ls` zeigt den Status des Compose-Projekts.
- `docker ps --filter label=shs.role` listet verwaltete Container und deren Zustand auf.
- `Get-Service Ollama` in PowerShell bestätigt den nativen Ollama-Dienst.

## Logs
- Worker (Live-Stream): `docker compose -f docker/compose.yaml logs shs-worker`.
- Worker (persistentes JSON): `${LOG_DIR:-../data/logs}/worker/worker-<Datum>.json` (PowerShell-Helfer: `Get-Content -Path (Join-Path $PWD '..\data\logs\worker\worker-*.json') -Wait`, wenn der Standardpfad genutzt wird).
- Service-Archive: `${LOG_DIR:-../data/logs}/services/<service>.log` enthält das jüngste geharvestete Stdout pro Container. Dateien rotieren automatisch bei ~10 MB oder nach 7 Tagen und behalten standardmäßig fünf archivierte Versionen (`LogCollector:Rotation` in `appsettings.json`).
- OpenWebUI (live): `docker logs shs-stack-open-webui-1`.
- Stable Diffusion (live): `docker logs shs-stack-automatic1111-1`.

## Neustartrichtlinie
- Einzelnen Dienst neu starten: `docker compose -f docker/compose.yaml restart <service>`.
- Gesamten Stack recyceln: `docker compose -f docker/compose.yaml down` gefolgt von `up -d`.
- GPU-Druck: Schwere Jobs pausieren, indem der Stable-Diffusion-Container gestoppt wird (`docker compose stop automatic1111`); der Worker erfasst konfigurierte Limits, setzt sie aber nicht durch.

## Health Checks
- Worker: `curl http://localhost:${WORKER_HEALTH_PORT:-5080}/health` für HTTP 200 oder `curl http://localhost:${WORKER_HEALTH_PORT:-5080}/live` für eine minimale JSON-Liveness-Antwort.
- OpenWebUI: `curl http://localhost:3003/api/system/info`.
- Qdrant: `curl http://localhost:6334/readyz`.
- AUTOMATIC1111: `curl -H "Content-Type: application/json" -X POST http://localhost:7860/sdapi/v1/txt2img -d '{"prompt":"test","steps":1,"width":64,"height":64}'`.

## Datenpfade
- `docker/.env` definiert Host-Pfade für Volumes.
- Persistente Daten liegen unter `data/open-webui`, `data/qdrant`, `data/automatic1111`.
- Model-Cache befindet sich in `models/stable-diffusion`.
- Logs persistieren unter `${LOG_DIR:-../data/logs}` mit den Unterordnern `worker`, `services` und `state`.

## Häufige Probleme
- **Permission denied** bei `git add`: Visual Studio schließen oder `.vs/` ignoriert lassen.
- **GPU nicht sichtbar**: `nvidia-smi` ausführen und GPU-Support in Docker Desktop aktivieren.
- **Dienst fehlt**: Sicherstellen, dass das Label `shs.role` existiert und `Docker:Detection:LabelSelector` entspricht.
- **Docker-CLI fehlt**: Der Worker benötigt Zugriff auf das `docker`-Binary. Im Containerbetrieb den Mount `/var/run/docker.sock` beibehalten; beim nativen Betrieb Docker Desktop installieren und sicherstellen, dass `docker` im `PATH` liegt.
- **AUTOMATIC1111 Options-API schlägt fehl** (`sd_model_checkpoint` KeyError): Der Stack führt `docker/automatic1111_patch.py` automatisch aus, um Legacy-Images zu korrigieren; setze den Container mit `docker compose up -d --force-recreate automatic1111` neu auf, falls ein Rebuild den Patch entfernt.
