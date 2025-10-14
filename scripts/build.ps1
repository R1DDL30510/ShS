param(
    [switch]$SkipFormat
)

$ErrorActionPreference = 'Stop'
$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')

Push-Location $repoRoot
try {
    dotnet restore ShS.slnx

    if (-not $SkipFormat) {
        dotnet format ShS.slnx --verify-no-changes
    }

    dotnet build ShS.slnx --configuration Release
    dotnet test ShS.slnx --configuration Release --no-build --collect:'XPlat Code Coverage'
}
finally {
    Pop-Location
}
