namespace TeamsShortcuts.Core;

public enum LeaveConfirmationResult
{
    Armed,
    Confirmed
}

public sealed class LeaveConfirmationStateMachine(TimeSpan armWindow)
{
    private DateTimeOffset? _armedUntil;

    public bool IsArmed(DateTimeOffset now) => _armedUntil is not null && now <= _armedUntil.Value;

    public LeaveConfirmationResult Press(DateTimeOffset now)
    {
        if (IsArmed(now))
        {
            _armedUntil = null;
            return LeaveConfirmationResult.Confirmed;
        }

        _armedUntil = now.Add(armWindow);
        return LeaveConfirmationResult.Armed;
    }

    public void Reset() => _armedUntil = null;
}
