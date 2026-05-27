namespace TeamsShortcuts.Core;

public sealed class Win32WindowFocusService(TeamsWindowFinder finder, ITeamsShortcutLogger? log = null) : IWindowFocusService
{
    private readonly ITeamsShortcutLogger _log = log ?? NullTeamsShortcutLogger.Instance;

    public IntPtr GetForegroundWindow() => Win32.GetForegroundWindowHandle();

    public Task<bool> TryFocusTeamsAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var candidate = finder.FindBestTeamsMeetingWindow();
        if (candidate is null)
        {
            _log.Warn("No Microsoft Teams window was found to focus.");
            return Task.FromResult(false);
        }

        _log.Verbose("Focusing Teams window:");
        _log.Verbose(TeamsWindowFinder.Describe(candidate));
        return Task.FromResult(Win32.BringToForeground(candidate.Window.Hwnd));
    }

    public Task<bool> TryRestoreForegroundWindowAsync(IntPtr windowHandle, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(Win32.RestoreForegroundWindow(windowHandle));
    }
}

public sealed class Win32KeyboardInputService : IKeyboardInputService
{
    public Task SendShortcutAsync(KeyboardShortcut shortcut, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var inputs = new List<Win32.INPUT>();

        foreach (var modifier in shortcut.Modifiers)
        {
            inputs.Add(Win32.KeyDownInput(ToVirtualKey(modifier)));
        }

        inputs.Add(Win32.KeyDownInput(ToVirtualKey(shortcut.Key)));
        inputs.Add(Win32.KeyUpInput(ToVirtualKey(shortcut.Key)));

        foreach (var modifier in shortcut.Modifiers.Reverse())
        {
            inputs.Add(Win32.KeyUpInput(ToVirtualKey(modifier)));
        }

        Win32.SendKeyboardInputs(inputs);
        return Task.CompletedTask;
    }

    public Task KeyDownAsync(KeyboardShortcut shortcut, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        foreach (var modifier in shortcut.Modifiers)
        {
            Win32.SendKeyDown(ToVirtualKey(modifier));
        }

        Win32.SendKeyDown(ToVirtualKey(shortcut.Key));
        return Task.CompletedTask;
    }

    public Task KeyUpAsync(KeyboardShortcut shortcut, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Exception? firstException = null;

        try
        {
            Win32.SendKeyUp(ToVirtualKey(shortcut.Key));
        }
        catch (Exception ex)
        {
            firstException = ex;
        }

        foreach (var modifier in shortcut.Modifiers.Reverse())
        {
            try
            {
                Win32.SendKeyUp(ToVirtualKey(modifier));
            }
            catch (Exception ex) when (firstException is null)
            {
                firstException = ex;
            }
        }

        if (firstException is not null)
        {
            throw firstException;
        }

        return Task.CompletedTask;
    }

    private static ushort ToVirtualKey(KeyboardModifier modifier)
    {
        return modifier switch
        {
            KeyboardModifier.Control => 0x11,
            KeyboardModifier.Shift => 0x10,
            KeyboardModifier.Alt => 0x12,
            KeyboardModifier.Windows => 0x5B,
            _ => throw new ArgumentOutOfRangeException(nameof(modifier), modifier, null)
        };
    }

    private static ushort ToVirtualKey(KeyboardKey key)
    {
        return key switch
        {
            >= KeyboardKey.A and <= KeyboardKey.Z => (ushort)('A' + (key - KeyboardKey.A)),
            >= KeyboardKey.D0 and <= KeyboardKey.D9 => (ushort)('0' + (key - KeyboardKey.D0)),
            KeyboardKey.Space => 0x20,
            KeyboardKey.Enter => 0x0D,
            KeyboardKey.Escape => 0x1B,
            >= KeyboardKey.F1 and <= KeyboardKey.F12 => (ushort)(0x70 + (key - KeyboardKey.F1)),
            _ => throw new ArgumentOutOfRangeException(nameof(key), key, null)
        };
    }
}
