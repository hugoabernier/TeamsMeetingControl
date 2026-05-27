using TeamsShortcuts.Core;

namespace TeamsShortcuts.Core.Tests;

public sealed class LeaveConfirmationStateMachineTests
{
    [Fact]
    public void FirstPressArmsAndSecondPressConfirms()
    {
        var now = new DateTimeOffset(2026, 5, 27, 12, 0, 0, TimeSpan.Zero);
        var confirmation = new LeaveConfirmationStateMachine(TimeSpan.FromSeconds(3));

        Assert.Equal(LeaveConfirmationResult.Armed, confirmation.Press(now));
        Assert.True(confirmation.IsArmed(now.AddSeconds(2)));
        Assert.Equal(LeaveConfirmationResult.Confirmed, confirmation.Press(now.AddSeconds(2)));
        Assert.False(confirmation.IsArmed(now.AddSeconds(2)));
    }

    [Fact]
    public void PressAfterWindowRearmsInsteadOfConfirming()
    {
        var now = new DateTimeOffset(2026, 5, 27, 12, 0, 0, TimeSpan.Zero);
        var confirmation = new LeaveConfirmationStateMachine(TimeSpan.FromSeconds(3));

        Assert.Equal(LeaveConfirmationResult.Armed, confirmation.Press(now));
        Assert.Equal(LeaveConfirmationResult.Armed, confirmation.Press(now.AddSeconds(4)));
    }
}
