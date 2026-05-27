# Teams Shortcuts Stream Deck Plugin

Teams Shortcuts is a Windows-only C# Stream Deck plugin for Microsoft Teams meeting keyboard controls.

It sends documented Teams keyboard shortcuts and Windows input commands. It does not use a Microsoft Teams API, Microsoft Graph, private Teams APIs, cloud services, or telemetry. Treat every button as a command button, not as a state indicator.

## Actions

- `Mute`: toggles mute with `Ctrl+Shift+M`.
- `Camera`: toggles camera with `Ctrl+Shift+O`.
- `Share`: opens Teams share controls with `Ctrl+Shift+E`.
- `Raise`: raises or lowers hand with `Ctrl+Shift+K`.
- `Captions`: toggles live captions with `Ctrl+Shift+A`.
- `Hold Talk`: holds `Ctrl+Spacebar` while the Stream Deck key is held.
- `Leave`: press once to arm, press again within 3 seconds to leave/end the meeting with `Ctrl+Shift+H`.
- `Like`, `Love`, `Laugh`, `Wow`, `Applaud`: sends Teams live reactions using the shared Teams reaction controller from the console POC.

## Prerequisites

- Windows 10 or later.
- Microsoft Teams desktop app.
- Stream Deck desktop app.
- .NET 8 SDK to build from source.

## Build

From the repository root:

```powershell
.\build\Build-TeamsShortcutsPlugin.ps1
```

The ready-to-link plugin folder is:

```text
streamdeck\TeamsShortcuts\com.hugobernier.teams-shortcuts.sdPlugin
```

To package with the Stream Deck CLI if it is installed:

```powershell
.\build\Build-TeamsShortcutsPlugin.ps1 -Pack
```

## Sideload

For development, link or copy the `.sdPlugin` folder into the Stream Deck plugins folder and restart Stream Deck. If you packed the plugin with the Stream Deck CLI, double-click the generated `.streamDeckPlugin` file to install it.

This plugin does not require the Elgato Marketplace.

## Settings

The shared core supports these focus modes:

- `ActiveWindow`: send to the currently focused window.
- `FocusTeams`: focus Microsoft Teams first.
- `FocusTeamsAndRestorePrevious`: focus Teams, send the shortcut, then try to restore the previous foreground window.

The first pass does not include a JavaScript property inspector because this plugin is intentionally C#-only. The C# settings model is in place so a future property inspector can configure focus mode and focus delay without changing the command service.

## Troubleshooting

- Teams must be running.
- Some shortcuts require a Teams meeting window or Teams focus.
- Elevated/non-elevated app mismatch can prevent input from being delivered.
- If Stream Deck is running as administrator and Teams is not, or vice versa, input behavior may be inconsistent.
- The plugin cannot guarantee mute/camera state unless state detection is explicitly implemented later.
- Push-to-talk depends on Teams supporting `Ctrl+Spacebar` hold behavior and on Teams privacy settings that allow temporary unmute.
- Reactions use bounded UI Automation against the selected Teams window and can fail if Teams changes accessible names.

## Manual Test Checklist

1. Build the plugin.
2. Link or sideload the `.sdPlugin` folder.
3. Add each action to a Stream Deck profile.
4. Open Microsoft Teams.
5. Join a test meeting.
6. Press Mute and verify Teams toggles mute.
7. Press Camera and verify Teams toggles camera.
8. Press Share and verify Teams opens share controls.
9. Press Raise and verify Teams raises/lowers hand.
10. Press Captions and verify Teams toggles captions.
11. Hold Push-to-Talk and verify temporary unmute behavior.
12. Release Push-to-Talk and verify the keys are released.
13. Test Leave Meeting confirmation behavior carefully.
14. Test behavior when Teams is closed.
15. Test behavior when another app has focus.
16. Test each focus mode after settings UI is added.
