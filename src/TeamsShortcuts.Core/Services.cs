namespace TeamsShortcuts.Core;

public interface ITeamsCommandService
{
    Task SendCommandAsync(TeamsCommand command, TeamsCommandOptions options, CancellationToken cancellationToken);
    Task SendShortcutAsync(KeyboardShortcut shortcut, TeamsCommandOptions options, CancellationToken cancellationToken);
    Task BeginTemporaryUnmuteAsync(TeamsCommandOptions options, CancellationToken cancellationToken);
    Task EndTemporaryUnmuteAsync(TeamsCommandOptions options, CancellationToken cancellationToken);
}

public interface IKeyboardInputService
{
    Task SendShortcutAsync(KeyboardShortcut shortcut, CancellationToken cancellationToken);
    Task KeyDownAsync(KeyboardShortcut shortcut, CancellationToken cancellationToken);
    Task KeyUpAsync(KeyboardShortcut shortcut, CancellationToken cancellationToken);
}

public interface IWindowFocusService
{
    IntPtr GetForegroundWindow();
    Task<bool> TryFocusTeamsAsync(CancellationToken cancellationToken);
    Task<bool> TryRestoreForegroundWindowAsync(IntPtr windowHandle, CancellationToken cancellationToken);
}

public interface ITeamsReactionService
{
    Task SendReactionAsync(TeamsReaction reaction, CancellationToken cancellationToken);
}

public sealed class TeamsWindowNotFoundException() : InvalidOperationException("Microsoft Teams window was not found.");

public sealed class TeamsCommandService(
    IKeyboardInputService keyboard,
    IWindowFocusService focus,
    ITeamsReactionService reactions,
    ITeamsShortcutLogger? log = null) : ITeamsCommandService
{
    private readonly ITeamsShortcutLogger _log = log ?? NullTeamsShortcutLogger.Instance;

    public async Task SendCommandAsync(TeamsCommand command, TeamsCommandOptions options, CancellationToken cancellationToken)
    {
        _log.Verbose($"Sending Teams command: {command}; focus mode: {options.FocusMode}");

        if (TeamsCommandShortcuts.TryGetReaction(command, out var reaction))
        {
            await SendWithFocusAsync(options, () => reactions.SendReactionAsync(reaction, cancellationToken), cancellationToken);
            return;
        }

        if (!TeamsCommandShortcuts.TryGetShortcut(command, out var shortcut))
        {
            throw new NotSupportedException($"Unsupported Teams command: {command}");
        }

        if (command == TeamsCommand.TemporaryUnmuteDown)
        {
            await BeginTemporaryUnmuteAsync(options, cancellationToken);
            return;
        }

        if (command == TeamsCommand.TemporaryUnmuteUp)
        {
            await EndTemporaryUnmuteAsync(options, cancellationToken);
            return;
        }

        await SendShortcutAsync(shortcut, options, cancellationToken);
    }

    public Task SendShortcutAsync(KeyboardShortcut shortcut, TeamsCommandOptions options, CancellationToken cancellationToken)
    {
        return SendWithFocusAsync(options, () => keyboard.SendShortcutAsync(shortcut, cancellationToken), cancellationToken);
    }

    public Task BeginTemporaryUnmuteAsync(TeamsCommandOptions options, CancellationToken cancellationToken)
    {
        var shortcut = new KeyboardShortcut
        {
            Modifiers = [KeyboardModifier.Control],
            Key = KeyboardKey.Space
        };

        return SendWithFocusAsync(options, () => keyboard.KeyDownAsync(shortcut, cancellationToken), cancellationToken);
    }

    public Task EndTemporaryUnmuteAsync(TeamsCommandOptions options, CancellationToken cancellationToken)
    {
        var shortcut = new KeyboardShortcut
        {
            Modifiers = [KeyboardModifier.Control],
            Key = KeyboardKey.Space
        };

        return SendWithFocusAsync(options, () => keyboard.KeyUpAsync(shortcut, cancellationToken), cancellationToken);
    }

    private async Task SendWithFocusAsync(TeamsCommandOptions options, Func<Task> send, CancellationToken cancellationToken)
    {
        if (options.FocusMode == TeamsFocusMode.ActiveWindow)
        {
            await send();
            return;
        }

        var previous = focus.GetForegroundWindow();
        if (!await focus.TryFocusTeamsAsync(cancellationToken))
        {
            throw new TeamsWindowNotFoundException();
        }

        if (options.DelayAfterFocusMilliseconds > 0)
        {
            await Task.Delay(options.DelayAfterFocusMilliseconds, cancellationToken);
        }

        try
        {
            await send();
        }
        finally
        {
            if (options.FocusMode == TeamsFocusMode.FocusTeamsAndRestorePrevious && previous != IntPtr.Zero)
            {
                _ = await focus.TryRestoreForegroundWindowAsync(previous, cancellationToken);
            }
        }
    }
}
