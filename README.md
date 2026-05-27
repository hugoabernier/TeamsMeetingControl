# Teams Meeting Control

Windows-only C# proof of concept and Stream Deck plugin for Microsoft Teams keyboard controls.

- Shared command logic: [src/TeamsShortcuts.Core](src/TeamsShortcuts.Core)
- Diagnostic CLI: [src/TeamsShortcuts.Console](src/TeamsShortcuts.Console)
- Stream Deck plugin: [streamdeck/TeamsShortcuts](streamdeck/TeamsShortcuts)

Build everything:

```powershell
dotnet build .\TeamsMeetingControl.sln -c Release -p:Platform=x64
```

Run tests:

```powershell
dotnet test .\tests\TeamsShortcuts.Core.Tests\TeamsShortcuts.Core.Tests.csproj -c Release -p:Platform=x64
```

Build the ready-to-link Stream Deck plugin folder:

```powershell
.\build\Build-TeamsShortcutsPlugin.ps1
```

Build and package a sideloadable Stream Deck plugin:

```powershell
.\build\Build-TeamsShortcutsPlugin.ps1 -Pack
```

The package output is:

```text
streamdeck\TeamsShortcuts\net.timerinator.teams-shortcuts.streamDeckPlugin
```

Build the downloadable release artifact:

```powershell
.\scripts\package-plugin.ps1 -Version 0.1.0 -Clean
```

The release copy is written to:

```text
artifacts\releases\net.timerinator.teams-shortcuts.streamDeckPlugin
```

To publish the plugin for the Timerinator website, run the `Build Teams Shortcuts plugin` GitHub Actions workflow with the desired version, or push a `v*.*.*` tag. The workflow creates or updates a GitHub release containing:

```text
net.timerinator.teams-shortcuts.streamDeckPlugin
```
