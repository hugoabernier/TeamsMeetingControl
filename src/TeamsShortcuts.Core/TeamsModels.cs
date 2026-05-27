namespace TeamsShortcuts.Core;

public enum TeamsCommand
{
    ToggleMute,
    ToggleCamera,
    ToggleShare,
    ToggleHand,
    ToggleLiveCaptions,
    TemporaryUnmuteDown,
    TemporaryUnmuteUp,
    LeaveMeeting,
    ReactLike,
    ReactLove,
    ReactApplause,
    ReactLaugh,
    ReactSurprised
}

public enum TeamsReaction
{
    Like,
    Love,
    Applause,
    Laugh,
    Surprised
}

public enum TeamsFocusMode
{
    ActiveWindow,
    FocusTeams,
    FocusTeamsAndRestorePrevious
}

public sealed record TeamsCommandOptions
{
    public TeamsFocusMode FocusMode { get; init; } = TeamsFocusMode.FocusTeams;
    public int DelayAfterFocusMilliseconds { get; init; } = 100;
}

public static class TeamsCommandShortcuts
{
    public static bool TryGetShortcut(TeamsCommand command, out KeyboardShortcut shortcut)
    {
        shortcut = command switch
        {
            TeamsCommand.ToggleMute => CtrlShift(KeyboardKey.M),
            TeamsCommand.ToggleCamera => CtrlShift(KeyboardKey.O),
            TeamsCommand.ToggleShare => CtrlShift(KeyboardKey.E),
            TeamsCommand.ToggleHand => CtrlShift(KeyboardKey.K),
            TeamsCommand.ToggleLiveCaptions => CtrlShift(KeyboardKey.A),
            TeamsCommand.TemporaryUnmuteDown => new KeyboardShortcut
            {
                Modifiers = [KeyboardModifier.Control],
                Key = KeyboardKey.Space
            },
            TeamsCommand.TemporaryUnmuteUp => new KeyboardShortcut
            {
                Modifiers = [KeyboardModifier.Control],
                Key = KeyboardKey.Space
            },
            TeamsCommand.LeaveMeeting => CtrlShift(KeyboardKey.H),
            _ => new KeyboardShortcut { Modifiers = [], Key = KeyboardKey.Space }
        };

        return command is not (
            TeamsCommand.ReactLike or
            TeamsCommand.ReactLove or
            TeamsCommand.ReactApplause or
            TeamsCommand.ReactLaugh or
            TeamsCommand.ReactSurprised);
    }

    public static bool TryGetReaction(TeamsCommand command, out TeamsReaction reaction)
    {
        reaction = command switch
        {
            TeamsCommand.ReactLike => TeamsReaction.Like,
            TeamsCommand.ReactLove => TeamsReaction.Love,
            TeamsCommand.ReactApplause => TeamsReaction.Applause,
            TeamsCommand.ReactLaugh => TeamsReaction.Laugh,
            TeamsCommand.ReactSurprised => TeamsReaction.Surprised,
            _ => default
        };

        return command is
            TeamsCommand.ReactLike or
            TeamsCommand.ReactLove or
            TeamsCommand.ReactApplause or
            TeamsCommand.ReactLaugh or
            TeamsCommand.ReactSurprised;
    }

    private static KeyboardShortcut CtrlShift(KeyboardKey key)
    {
        return new KeyboardShortcut
        {
            Modifiers = [KeyboardModifier.Control, KeyboardModifier.Shift],
            Key = key
        };
    }
}
