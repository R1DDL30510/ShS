# SecureHomeSystem Testleitfaden

Dieses Dokument fasst die automatisierten Teststufen zusammen und erklärt, wie sie lokal ausgeführt werden.

## Voraussetzungen

- .NET SDK 9.0 oder neuer
- Docker-CLI (nur für Smoke-Tests erforderlich)

## Testkategorien

| Kategorie      | Beschreibung                                                                 | Befehl |
|---------------|------------------------------------------------------------------------------|--------|
| `Unit`        | Schnelle Tests, die externe Prozesse (z. B. Docker-Erkennung) mocken.        | `dotnet test ShS.slnx --configuration Release --filter "Category=Unit"` |
| `Integration` | Dateisystemintensive Tests zur Überprüfung von Log-Cursor-Persistenz und Rotation. | `dotnet test ShS.slnx --configuration Release --filter "Category=Integration"` |
| `Smoke`       | End-to-End-Verifikation des Docker-Compose-Stacks (optional, nicht Teil der Standardsuite). | `pwsh ./scripts/smoke.ps1` |

## Standard-Testlauf

Das Repository enthält `scripts/build.ps1`, das die Lösung wiederherstellt, formatiert, baut und testet:

```powershell
pwsh ./scripts/build.ps1
```

Während der Iteration lässt sich die Formatierung überspringen:

```powershell
pwsh ./scripts/build.ps1 -SkipFormat
```

## Smoke-Tests

Smoke-Tests starten den Stack mit Docker Compose und führen anschließend nur Tests aus, die mit `Category=Smoke` markiert sind. Sie sind optional, weil viele CI-Umgebungen kein Docker bereitstellen. Manuelle Ausführung:

```powershell
pwsh ./scripts/smoke.ps1
```

Übergebe `-SkipTeardown`, um die Container nach dem Lauf zu inspizieren.

## Continuous Integration

Der GitHub-Actions-Workflow (`.github/workflows/ci.yml`) führt das Build-Skript auf Windows und Linux aus. Über den Button „Run workflow“ in GitHub und den Parameter `run_smoke=true` lassen sich die Docker-basierten Smoke-Jobs einschließen.
