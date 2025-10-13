# Operations-Runbook

## Statusprüfungen
- `docker compose -f docker/compose.yaml ls` zeigt den Status des Compose-Projekts.
- `docker ps --filter label=shs.role` listet verwaltete Container und deren Zustand auf.
- `Get-Service Ollama` in PowerShell bestätigt den nativen Ollama-Dienst.

## Logs
- Worker: `docker compose logs shs-worker`. ⚠️ Weiterleitung in das Windows-Ereignisprotokoll ist im aktuellen Worker-Build **nicht** konfiguriert; passen Sie diese Anleitung an, falls ein nativer Windows-Dienst hinzukommt.
- OpenWebUI: `docker logs shs-stack-open-webui-1`.
- Stable Diffusion: `docker logs shs-stack-automatic1111-1`.

## Neustartrichtlinie
- Einzelnen Dienst neu starten: `docker compose -f docker/compose.yaml restart <service>`.
- Gesamten Stack recyceln: `docker compose -f docker/compose.yaml down` gefolgt von `up -d`.
- GPU-Druck: Schwere Jobs pausieren, indem der Stable-Diffusion-Container gestoppt wird (`docker compose stop automatic1111`); der Worker erfasst konfigurierte Limits, setzt sie aber nicht durch.

## Health Checks
- ⚠️ HTTP-Health-Endpoint des Workers ist **noch nicht implementiert**. Ersetzen Sie diesen Platzhalter, sobald eine Probe verfügbar ist.
- OpenWebUI: `curl http://localhost:3003/api/system/info`.
- Qdrant: `curl http://localhost:6334/readyz`.
- AUTOMATIC1111: `curl -X POST http://localhost:7860/sdapi/v1/txt2img -d '{"prompt":"test","steps":1,"width":64,"height":64}'`.

## Datenpfade
- `docker/.env` definiert Hostpfade für Volumes.
- Persistente Daten liegen unter `data/open-webui`, `data/qdrant`, `data/automatic1111`.
- Modell-Cache befindet sich in `models/stable-diffusion`.

## Häufige Probleme
- **Zugriff verweigert** bei `git add`: Visual Studio schließen oder `.vs/` ignoriert lassen.
- **GPU nicht sichtbar**: `nvidia-smi` ausführen und GPU-Unterstützung in Docker Desktop aktivieren.
- **Dienst fehlt**: sicherstellen, dass das Label `shs.role` vorhanden ist und zu `Docker:Detection:LabelSelector` passt.
- **Docker-CLI fehlt**: Der Worker benötigt Zugriff auf das `docker`-Binary. Im Containerbetrieb das Mount `/var/run/docker.sock` beibehalten; bei nativer Ausführung sicherstellen, dass Docker Desktop installiert ist und `docker` im `PATH` liegt.
- **AUTOMATIC1111 Options-API schlägt fehl** (`sd_model_checkpoint` KeyError): Der Stack führt automatisch `docker/automatic1111_patch.py` aus, um Legacy-Images zu korrigieren; starten Sie den Container mit `docker compose up -d --force-recreate automatic1111` neu, wenn ein Rebuild den Patch entfernt hat.
