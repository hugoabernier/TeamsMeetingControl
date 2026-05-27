using TeamsShortcuts.Core;

namespace TeamsShortcuts.Core.Tests;

public sealed class TeamsCommandShortcutsTests
{
    [Theory]
    [InlineData(TeamsCommand.ToggleMute, KeyboardKey.M)]
    [InlineData(TeamsCommand.ToggleCamera, KeyboardKey.O)]
    [InlineData(TeamsCommand.ToggleShare, KeyboardKey.E)]
    [InlineData(TeamsCommand.ToggleHand, KeyboardKey.K)]
    [InlineData(TeamsCommand.ToggleLiveCaptions, KeyboardKey.A)]
    [InlineData(TeamsCommand.LeaveMeeting, KeyboardKey.H)]
    public void CommandMapsToExpectedCtrlShiftShortcut(TeamsCommand command, KeyboardKey key)
    {
        Assert.True(TeamsCommandShortcuts.TryGetShortcut(command, out var shortcut));
        Assert.Equal([KeyboardModifier.Control, KeyboardModifier.Shift], shortcut.Modifiers);
        Assert.Equal(key, shortcut.Key);
    }

    [Fact]
    public void TemporaryUnmuteMapsToControlSpace()
    {
        Assert.True(TeamsCommandShortcuts.TryGetShortcut(TeamsCommand.TemporaryUnmuteDown, out var shortcut));
        Assert.Equal([KeyboardModifier.Control], shortcut.Modifiers);
        Assert.Equal(KeyboardKey.Space, shortcut.Key);
    }

    [Theory]
    [InlineData(TeamsCommand.ReactLike, TeamsReaction.Like)]
    [InlineData(TeamsCommand.ReactLove, TeamsReaction.Love)]
    [InlineData(TeamsCommand.ReactApplause, TeamsReaction.Applause)]
    [InlineData(TeamsCommand.ReactLaugh, TeamsReaction.Laugh)]
    [InlineData(TeamsCommand.ReactSurprised, TeamsReaction.Surprised)]
    public void ReactionCommandsMapToReactions(TeamsCommand command, TeamsReaction reaction)
    {
        Assert.True(TeamsCommandShortcuts.TryGetReaction(command, out var mapped));
        Assert.Equal(reaction, mapped);
    }
}
