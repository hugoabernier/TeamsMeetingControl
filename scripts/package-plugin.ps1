param(
    [ValidatePattern('^\d+\.\d+\.\d+(\.\d+)?$')]
    [string]$Version = "",

    [string]$Configuration = "Release",

    [switch]$Clean
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$releaseDir = Join-Path $repoRoot "artifacts\releases"
$packageName = "net.timerinator.teams-shortcuts.streamDeckPlugin"
$packagePath = Join-Path $repoRoot "streamdeck\TeamsShortcuts\$packageName"

if ($Clean) {
    Remove-Item -LiteralPath $releaseDir -Recurse -Force -ErrorAction SilentlyContinue
}

New-Item -ItemType Directory -Path $releaseDir -Force | Out-Null

if (![string]::IsNullOrWhiteSpace($Version)) {
    & (Join-Path $repoRoot "build\Build-TeamsShortcutsPlugin.ps1") -Configuration $Configuration -PluginVersion $Version -Pack
}
else {
    & (Join-Path $repoRoot "build\Build-TeamsShortcutsPlugin.ps1") -Configuration $Configuration -Pack
}

if (!(Test-Path -LiteralPath $packagePath)) {
    throw "Expected Stream Deck package was not created: $packagePath"
}

Copy-Item -LiteralPath $packagePath -Destination (Join-Path $releaseDir $packageName) -Force
Write-Host "Teams Shortcuts release file is in: $releaseDir"
