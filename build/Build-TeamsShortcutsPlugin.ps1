param(
    [string]$Configuration = "Release",
    [string]$PluginVersion = "",
    [switch]$Pack
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$project = Join-Path $repoRoot "streamdeck/TeamsShortcuts/TeamsShortcuts.StreamDeck.csproj"
$pluginRoot = Join-Path $repoRoot "streamdeck/TeamsShortcuts/net.timerinator.teams-shortcuts.sdPlugin"
$publishDir = Join-Path $repoRoot "streamdeck/TeamsShortcuts/bin/publish/win-x64"

if (-not (Test-Path $project)) {
    throw "Plugin project not found: $project"
}

if (-not (Test-Path (Join-Path $pluginRoot "manifest.json"))) {
    throw "manifest.json not found in plugin folder: $pluginRoot"
}

if (![string]::IsNullOrWhiteSpace($PluginVersion)) {
    if ($PluginVersion -notmatch '^\d+\.\d+\.\d+(\.\d+)?$') {
        throw "PluginVersion must use numeric version format, for example 0.1.2 or 0.1.2.0."
    }

    if ($PluginVersion -match '^\d+\.\d+\.\d+$') {
        $PluginVersion = "$PluginVersion.0"
    }

    $manifestPath = Join-Path $pluginRoot "manifest.json"
    $manifest = Get-Content -Raw -LiteralPath $manifestPath | ConvertFrom-Json
    $manifest.Version = $PluginVersion
    $manifest | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $manifestPath -Encoding UTF8
    Write-Host "Updated Stream Deck manifest version to $PluginVersion"
}

Write-Host "Generating Stream Deck high-contrast PNG icons..."
& (Join-Path $PSScriptRoot "Generate-TeamsShortcutsIcons.ps1") | Out-Host

Write-Host "Cleaning previous plugin publish output..."
if (Test-Path $publishDir) {
    Remove-Item -LiteralPath $publishDir -Recurse -Force
}

Write-Host "Publishing Teams Shortcuts Stream Deck plugin..."
dotnet publish $project -c $Configuration -r win-x64 --self-contained false -o $publishDir

Write-Host "Copying published files into .sdPlugin folder..."
Get-ChildItem -LiteralPath $publishDir -File | ForEach-Object {
    Copy-Item -LiteralPath $_.FullName -Destination $pluginRoot -Force
}

$requiredFiles = @(
    "manifest.json",
    "TeamsShortcuts.StreamDeck.exe",
    "TeamsShortcuts.StreamDeck.dll",
    "TeamsShortcuts.Core.dll",
    "Images/pluginIcon.png",
    "Images/pluginIcon@2x.png",
    "Images/mute.png",
    "Images/mute@2x.png",
    "Images/camera.png",
    "Images/share.png",
    "Images/leave.png",
    "Images/react-like.png",
    "Images/react-love.png",
    "Images/react-laugh.png",
    "Images/react-surprised.png",
    "Images/react-applause.png"
)

foreach ($file in $requiredFiles) {
    $path = Join-Path $pluginRoot $file
    if (-not (Test-Path $path)) {
        throw "Required plugin output file is missing: $path"
    }
}

Write-Host "Ready-to-link plugin folder:"
Write-Host $pluginRoot

if ($Pack) {
    $streamdeck = Get-Command streamdeck -ErrorAction SilentlyContinue
    if ($null -eq $streamdeck) {
        Write-Warning "Stream Deck CLI was not found. Install it or run without -Pack."
    }
    else {
        Push-Location (Split-Path -Parent $pluginRoot)
        try {
            streamdeck pack ".\net.timerinator.teams-shortcuts.sdPlugin" --force
        }
        finally {
            Pop-Location
        }
    }
}
