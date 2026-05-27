using BarRaider.SdTools;
using TeamsShortcuts.Core;

namespace TeamsShortcuts.StreamDeck.Actions;

[PluginActionId(PluginIds.ToggleMute)]
internal sealed class ToggleMuteAction(SDConnection connection, InitialPayload payload)
    : TeamsCommandActionBase(connection, payload, "Mute")
{
    protected override TeamsCommand Command => TeamsCommand.ToggleMute;
}

[PluginActionId(PluginIds.ToggleCamera)]
internal sealed class ToggleCameraAction(SDConnection connection, InitialPayload payload)
    : TeamsCommandActionBase(connection, payload, "Camera")
{
    protected override TeamsCommand Command => TeamsCommand.ToggleCamera;
}

[PluginActionId(PluginIds.ToggleShare)]
internal sealed class ToggleShareAction(SDConnection connection, InitialPayload payload)
    : TeamsCommandActionBase(connection, payload, "Share")
{
    protected override TeamsCommand Command => TeamsCommand.ToggleShare;
}

[PluginActionId(PluginIds.ToggleHand)]
internal sealed class ToggleHandAction(SDConnection connection, InitialPayload payload)
    : TeamsCommandActionBase(connection, payload, "Raise")
{
    protected override TeamsCommand Command => TeamsCommand.ToggleHand;
}

[PluginActionId(PluginIds.ToggleCaptions)]
internal sealed class ToggleCaptionsAction(SDConnection connection, InitialPayload payload)
    : TeamsCommandActionBase(connection, payload, "Captions")
{
    protected override TeamsCommand Command => TeamsCommand.ToggleLiveCaptions;
}

[PluginActionId(PluginIds.ReactLike)]
internal sealed class ReactLikeAction(SDConnection connection, InitialPayload payload)
    : TeamsCommandActionBase(connection, payload, "Like")
{
    protected override TeamsCommand Command => TeamsCommand.ReactLike;
}

[PluginActionId(PluginIds.ReactLove)]
internal sealed class ReactLoveAction(SDConnection connection, InitialPayload payload)
    : TeamsCommandActionBase(connection, payload, "Love")
{
    protected override TeamsCommand Command => TeamsCommand.ReactLove;
}

[PluginActionId(PluginIds.ReactLaugh)]
internal sealed class ReactLaughAction(SDConnection connection, InitialPayload payload)
    : TeamsCommandActionBase(connection, payload, "Laugh")
{
    protected override TeamsCommand Command => TeamsCommand.ReactLaugh;
}

[PluginActionId(PluginIds.ReactSurprised)]
internal sealed class ReactSurprisedAction(SDConnection connection, InitialPayload payload)
    : TeamsCommandActionBase(connection, payload, "Wow")
{
    protected override TeamsCommand Command => TeamsCommand.ReactSurprised;
}

[PluginActionId(PluginIds.ReactApplause)]
internal sealed class ReactApplauseAction(SDConnection connection, InitialPayload payload)
    : TeamsCommandActionBase(connection, payload, "Applaud")
{
    protected override TeamsCommand Command => TeamsCommand.ReactApplause;
}
