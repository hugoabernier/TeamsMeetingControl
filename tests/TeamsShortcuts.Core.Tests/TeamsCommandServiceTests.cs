using TeamsShortcuts.Core;

namespace TeamsShortcuts.Core.Tests;

public sealed class TeamsCommandServiceTests
{
    [Fact]
    public async Task ActiveWindowDoesNotFocusTeams()
    {
        var keyboard = new RecordingKeyboardInputService();
        var focus = new RecordingWindowFocusService();
        var service = new TeamsCommandService(keyboard, focus, new RecordingReactionService());

        await service.SendCommandAsync(
            TeamsCommand.ToggleMute,
            new TeamsCommandOptions { FocusMode = TeamsFocusMode.ActiveWindow },
            CancellationToken.None);

        Assert.Equal(0, focus.FocusTeamsCalls);
        Assert.Equal("tap:Control+Shift+M", Assert.Single(keyboard.Events));
    }

    [Fact]
    public async Task FocusTeamsAndRestorePreviousRestoresAfterShortcut()
    {
        var keyboard = new RecordingKeyboardInputService();
        var focus = new RecordingWindowFocusService { Foreground = new IntPtr(1234) };
        var service = new TeamsCommandService(keyboard, focus, new RecordingReactionService());

        await service.SendCommandAsync(
            TeamsCommand.ToggleCamera,
            new TeamsCommandOptions { FocusMode = TeamsFocusMode.FocusTeamsAndRestorePrevious },
            CancellationToken.None);

        Assert.Equal(1, focus.FocusTeamsCalls);
        Assert.Equal(new IntPtr(1234), focus.RestoredWindow);
        Assert.Equal("tap:Control+Shift+O", Assert.Single(keyboard.Events));
    }

    [Fact]
    public async Task TemporaryUnmuteSendsKeyDownAndKeyUp()
    {
        var keyboard = new RecordingKeyboardInputService();
        var service = new TeamsCommandService(keyboard, new RecordingWindowFocusService(), new RecordingReactionService());
        var options = new TeamsCommandOptions { FocusMode = TeamsFocusMode.ActiveWindow };

        await service.BeginTemporaryUnmuteAsync(options, CancellationToken.None);
        await service.EndTemporaryUnmuteAsync(options, CancellationToken.None);

        Assert.Equal(["down:Control+Space", "up:Control+Space"], keyboard.Events);
    }

    [Fact]
    public async Task ReactionCommandDelegatesToReactionService()
    {
        var reactionService = new RecordingReactionService();
        var service = new TeamsCommandService(new RecordingKeyboardInputService(), new RecordingWindowFocusService(), reactionService);

        await service.SendCommandAsync(
            TeamsCommand.ReactLaugh,
            new TeamsCommandOptions { FocusMode = TeamsFocusMode.ActiveWindow },
            CancellationToken.None);

        Assert.Equal(TeamsReaction.Laugh, reactionService.LastReaction);
    }

    private sealed class RecordingKeyboardInputService : IKeyboardInputService
    {
        public List<string> Events { get; } = [];

        public Task SendShortcutAsync(KeyboardShortcut shortcut, CancellationToken cancellationToken)
        {
            Events.Add("tap:" + shortcut);
            return Task.CompletedTask;
        }

        public Task KeyDownAsync(KeyboardShortcut shortcut, CancellationToken cancellationToken)
        {
            Events.Add("down:" + shortcut);
            return Task.CompletedTask;
        }

        public Task KeyUpAsync(KeyboardShortcut shortcut, CancellationToken cancellationToken)
        {
            Events.Add("up:" + shortcut);
            return Task.CompletedTask;
        }
    }

    private sealed class RecordingWindowFocusService : IWindowFocusService
    {
        public IntPtr Foreground { get; init; } = new(42);
        public int FocusTeamsCalls { get; private set; }
        public IntPtr? RestoredWindow { get; private set; }

        public IntPtr GetForegroundWindow() => Foreground;

        public Task<bool> TryFocusTeamsAsync(CancellationToken cancellationToken)
        {
            FocusTeamsCalls++;
            return Task.FromResult(true);
        }

        public Task<bool> TryRestoreForegroundWindowAsync(IntPtr windowHandle, CancellationToken cancellationToken)
        {
            RestoredWindow = windowHandle;
            return Task.FromResult(true);
        }
    }

    private sealed class RecordingReactionService : ITeamsReactionService
    {
        public TeamsReaction? LastReaction { get; private set; }

        public Task SendReactionAsync(TeamsReaction reaction, CancellationToken cancellationToken)
        {
            LastReaction = reaction;
            return Task.CompletedTask;
        }
    }
}
