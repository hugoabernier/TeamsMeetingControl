using BarRaider.SdTools;
using TeamsShortcuts.Core;

namespace TeamsShortcuts.StreamDeck.Actions;

[PluginActionId(PluginIds.LeaveMeeting)]
internal sealed class LeaveMeetingAction : TeamsCommandActionBase
{
    private readonly LeaveConfirmationStateMachine _confirmation = new(TimeSpan.FromSeconds(3));

    public LeaveMeetingAction(SDConnection connection, InitialPayload payload)
        : base(connection, payload, "Leave")
    {
    }

    protected override TeamsCommand Command => TeamsCommand.LeaveMeeting;

    public override void KeyPressed(KeyPayload payload)
    {
        var result = _confirmation.Press(DateTimeOffset.UtcNow);
        if (result == LeaveConfirmationResult.Armed)
        {
            _ = Connection.SetTitleAsync("Press\nAgain");
            return;
        }

        _ = ExecuteCommandAsync(TeamsCommand.LeaveMeeting, "Leave", CancellationToken.None);
    }

    public override void OnTick()
    {
        if (!_confirmation.IsArmed(DateTimeOffset.UtcNow))
        {
            _ = Connection.SetTitleAsync("Leave");
        }
    }
}
