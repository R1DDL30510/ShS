param(
    [string]$ComposeFile = (Join-Path (Join-Path $PSScriptRoot '..') 'docker/compose.yaml'),
    [string]$ServiceName = 'shs-worker',
    [switch]$SkipTeardown
)

$ErrorActionPreference = 'Stop'
$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')

if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
    Write-Warning 'Docker CLI not found. Skipping smoke tests.'
    exit 0
}

Push-Location $repoRoot
try {
    docker compose -f $ComposeFile pull
    docker compose -f $ComposeFile up --wait $ServiceName

    dotnet test ShS.slnx --configuration Release --no-build --filter 'Category=Smoke'
}
finally {
    if (-not $SkipTeardown.IsPresent) {
        docker compose -f $ComposeFile down --remove-orphans
    }

    Pop-Location
}
