$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$imageDir = Join-Path $repoRoot "streamdeck/TeamsShortcuts/net.timerinator.teams-shortcuts.sdPlugin/Images"

New-Item -ItemType Directory -Path $imageDir -Force | Out-Null
Get-ChildItem -LiteralPath $imageDir -Filter "*.png" -ErrorAction SilentlyContinue | Remove-Item -Force
Get-ChildItem -LiteralPath $imageDir -Filter "*.svg" -ErrorAction SilentlyContinue | Remove-Item -Force

function Write-PathIcon {
    param(
        [string]$Name,
        [string]$ViewBox,
        [string]$PathData,
        [string]$Background,
        [int]$Inset = 30
    )

    $escapedPath = [System.Security.SecurityElement]::Escape($PathData)
    $svg = @"
<svg xmlns="http://www.w3.org/2000/svg" width="144" height="144" viewBox="0 0 144 144">
  <rect width="144" height="144" rx="24" fill="$Background"/>
  <svg x="$Inset" y="$Inset" width="$(144 - ($Inset * 2))" height="$(144 - ($Inset * 2))" viewBox="$ViewBox">
    <path d="$escapedPath" fill="#ffffff"/>
  </svg>
</svg>
"@

    Set-Content -LiteralPath (Join-Path $imageDir "$Name.svg") -Value $svg -Encoding UTF8
}

function Write-EmojiIcon {
    param(
        [string]$Name,
        [string]$Emoji,
        [string]$Background
    )

    $svg = @"
<svg xmlns="http://www.w3.org/2000/svg" width="144" height="144" viewBox="0 0 144 144">
  <rect width="144" height="144" rx="24" fill="$Background"/>
  <text x="72" y="92" text-anchor="middle" font-family="Segoe UI Emoji, Apple Color Emoji, Noto Color Emoji, sans-serif" font-size="72">$Emoji</text>
</svg>
"@

    Set-Content -LiteralPath (Join-Path $imageDir "$Name.svg") -Value $svg -Encoding UTF8
}

function Write-TextIcon {
    param(
        [string]$Name,
        [string]$Text,
        [string]$Background,
        [string]$Foreground = "#ffffff",
        [int]$FontSize = 36
    )

    $escapedText = [System.Security.SecurityElement]::Escape($Text)
    $svg = @"
<svg xmlns="http://www.w3.org/2000/svg" width="144" height="144" viewBox="0 0 144 144">
  <rect width="144" height="144" rx="24" fill="$Background"/>
  <text x="72" y="84" text-anchor="middle" font-family="Segoe UI, Arial, sans-serif" font-size="$FontSize" font-weight="700" fill="$Foreground">$escapedText</text>
</svg>
"@

    Set-Content -LiteralPath (Join-Path $imageDir "$Name.svg") -Value $svg -Encoding UTF8
}

function Write-PushToTalkIcon {
    param(
        [string]$Name,
        [string]$Background
    )

    $svg = @"
<svg xmlns="http://www.w3.org/2000/svg" width="144" height="144" viewBox="0 0 144 144">
  <rect width="144" height="144" rx="24" fill="$Background"/>
  <svg x="24" y="24" width="96" height="96" viewBox="0 0 20 20">
    <path d="M5.5 10a.5.5 0 0 0-1 0 5.5 5.5 0 0 0 5 5.48v2.02a.5.5 0 0 0 1 0v-2.02c2.8-.26 5-2.61 5-5.48a.5.5 0 0 0-1 0 4.5 4.5 0 1 1-9 0Zm7.5 0a3 3 0 0 1-6 0V5a3 3 0 0 1 6 0v5Z" fill="#ffffff"/>
    <circle cx="15.75" cy="15.75" r="3.25" fill="#8DF0C4" fill-opacity="0.35"/>
    <path d="M15.75 13.55a.55.55 0 0 1 .55.55v1.46l.34-.34a.55.55 0 1 1 .78.78l-1.28 1.28a.55.55 0 0 1-.78 0L14.08 16a.55.55 0 0 1 .78-.78l.34.34V14.1a.55.55 0 0 1 .55-.55Z" fill="#8DF0C4"/>
  </svg>
</svg>
"@

    Set-Content -LiteralPath (Join-Path $imageDir "$Name.svg") -Value $svg -Encoding UTF8
}

$mutePath = "M13 10a3 3 0 0 1-.1.78L7 4.88A3 3 0 0 1 13 5v5ZM7 7.7V10a3 3 0 0 0 4.74 2.45l1.07 1.07A4.5 4.5 0 0 1 5.5 10a.5.5 0 0 0-1.01 0 5.5 5.5 0 0 0 5 5.48v2.02a.5.5 0 0 0 1 0v-2.02a5.48 5.48 0 0 0 3.02-1.25l3.63 3.62a.5.5 0 0 0 .7-.7l-15-15a.5.5 0 1 0-.7.7L7 7.71Zm7.8 4.98c.45-.8.7-1.7.7-2.68a.5.5 0 0 0-1 0c0 .7-.16 1.35-.44 1.94l.74.74Z"
$cameraPath = "M2.85 2.15a.5.5 0 1 0-.7.7l1.48 1.48A3 3 0 0 0 2 7v6a3 3 0 0 0 3 3h5a3 3 0 0 0 2.93-2.36l4.22 4.21a.5.5 0 0 0 .7-.7l-15-15ZM14 11.88l3.08 3.07c.5-.14.92-.6.92-1.2v-7.5c0-1-1.13-1.6-1.96-1.03L14 6.63v5.25ZM6.12 4 13 10.88V7a3 3 0 0 0-3-3H6.12Z"
$sharePath = "M4 4a2 2 0 0 0-2 2v8c0 1.1.9 2 2 2h12a2 2 0 0 0 2-2V6a2 2 0 0 0-2-2H4Zm6 10a.5.5 0 0 1-.5-.5V7.7L7.85 9.36a.5.5 0 1 1-.7-.7l2.5-2.5c.2-.2.5-.2.7 0l2.5 2.5a.5.5 0 0 1-.7.7L10.5 7.71v5.79a.5.5 0 0 1-.5.5Z"
$leavePath = "m17.96 10.94-.16.83c-.15.78-.87 1.3-1.7 1.22l-1.63-.16c-.72-.07-1.25-.59-1.47-1.33-.3-1-.5-1.75-.5-1.75a6.63 6.63 0 0 0-5 0s-.2.75-.5 1.75c-.2.67-.5 1.26-1.2 1.33l-1.63.16c-.81.08-1.6-.43-1.82-1.2l-.25-.84c-.25-.82-.03-1.7.58-2.28C4.1 7.3 6.67 6.51 9.99 6.5c3.33 0 5.6.78 7.16 2.16.66.58.97 1.46.8 2.28Z"
$handPath = "M4 12.02c0 1.06.2 2.1.6 3.08l.6 1.42c.22.55.64 1.01 1.17 1.29.27.14.56.21.86.21h2.55c.77 0 1.49-.41 1.87-1.08.5-.87 1.02-1.7 1.72-2.43l1.32-1.39c.44-.46.97-.84 1.49-1.23l.59-.45a.6.6 0 0 0 .23-.47c0-.75-.54-1.57-1.22-1.79a3.34 3.34 0 0 0-2.78.29V4.5a1.5 1.5 0 0 0-2.05-1.4 1.5 1.5 0 0 0-2.9 0A1.5 1.5 0 0 0 6 4.5v.09A1.5 1.5 0 0 0 4 6v6.02ZM8 4.5v4a.5.5 0 0 0 1 0v-5a.5.5 0 0 1 1 0v5a.5.5 0 0 0 1 0v-4a.5.5 0 0 1 1 0v6a.5.5 0 0 0 .85.37h.01c.22-.22.44-.44.72-.58.7-.35 2.22-.57 2.4.5l-.53.4c-.52.4-1.04.78-1.48 1.24l-1.33 1.38c-.75.79-1.31 1.7-1.85 2.63-.21.36-.6.58-1.01.58H7.23a.87.87 0 0 1-.4-.1 1.55 1.55 0 0 1-.71-.78l-.59-1.42a7.09 7.09 0 0 1-.53-2.7V6a.5.5 0 0 1 1 0v3.5a.5.5 0 0 0 1 0v-5a.5.5 0 0 1 1 0Z"
$captionsPath = "M1920 128v1408h-768v512l-384-512H0V128h1920zM960 1341q30 0 80-1t111-4 124-9 120-16 101-24 64-35q17-17 30-48t22-69 17-82 11-85 6-76 2-60q0-23-2-59t-6-77-11-85-17-83-23-71-29-48q-19-19-62-32t-101-23-122-16-125-9-110-5-80-1q-29 0-79 1t-111 4-125 10-122 15-100 23-63 33q-17 17-30 48t-22 70-17 82-11 85-6 77-2 60q0 24 2 59t6 77 10 85 17 82 23 70 30 48q15 15 43 26t61 20 63 13 52 8q95 12 190 17t191 5z"

Write-TextIcon -Name "pluginIcon" -Text "TK" -Background "#202124" -FontSize 36
Write-PathIcon -Name "mute" -ViewBox "0 0 20 20" -PathData $mutePath -Background "#7A1F1F" -Inset 24
Write-PathIcon -Name "camera" -ViewBox "0 0 20 20" -PathData $cameraPath -Background "#102A43" -Inset 24
Write-PathIcon -Name "share" -ViewBox "0 0 20 20" -PathData $sharePath -Background "#214E8A" -Inset 24
Write-PathIcon -Name "leave" -ViewBox "0 0 20 20" -PathData $leavePath -Background "#7A1F1F" -Inset 24
Write-PathIcon -Name "hand" -ViewBox "0 0 20 20" -PathData $handPath -Background "#2D3436" -Inset 24
Write-PathIcon -Name "captions" -ViewBox "0 0 2048 2048" -PathData $captionsPath -Background "#1B263B" -Inset 20
Write-PushToTalkIcon -Name "talk" -Background "#12372A"
$emojiLike = [char]::ConvertFromUtf32(0x1F44D)
$emojiLove = [char]::ConvertFromUtf32(0x2764) + [char]::ConvertFromUtf32(0xFE0F)
$emojiLaugh = [char]::ConvertFromUtf32(0x1F600)
$emojiSurprised = [char]::ConvertFromUtf32(0x1F62E)
$emojiApplause = [char]::ConvertFromUtf32(0x1F44F)

$reactionBackground = "#1F2937"
Write-EmojiIcon -Name "react-like" -Emoji $emojiLike -Background $reactionBackground
Write-EmojiIcon -Name "react-love" -Emoji $emojiLove -Background $reactionBackground
Write-EmojiIcon -Name "react-laugh" -Emoji $emojiLaugh -Background $reactionBackground
Write-EmojiIcon -Name "react-surprised" -Emoji $emojiSurprised -Background $reactionBackground
Write-EmojiIcon -Name "react-applause" -Emoji $emojiApplause -Background $reactionBackground

dotnet run --project (Join-Path $PSScriptRoot "TeamsShortcuts.IconGenerator/TeamsShortcuts.IconGenerator.csproj") -c Release -- $imageDir

Get-ChildItem -LiteralPath $imageDir -Filter "*.png" | Select-Object -ExpandProperty Name
