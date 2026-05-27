namespace TeamsShortcuts.Core;

public enum KeyboardModifier
{
    Control,
    Shift,
    Alt,
    Windows
}

public enum KeyboardKey
{
    A,
    B,
    C,
    D,
    E,
    F,
    G,
    H,
    I,
    J,
    K,
    L,
    M,
    N,
    O,
    P,
    Q,
    R,
    S,
    T,
    U,
    V,
    W,
    X,
    Y,
    Z,
    D0,
    D1,
    D2,
    D3,
    D4,
    D5,
    D6,
    D7,
    D8,
    D9,
    Space,
    Enter,
    Escape,
    F1,
    F2,
    F3,
    F4,
    F5,
    F6,
    F7,
    F8,
    F9,
    F10,
    F11,
    F12
}

public sealed record KeyboardShortcut
{
    public required IReadOnlyList<KeyboardModifier> Modifiers { get; init; }
    public required KeyboardKey Key { get; init; }

    public override string ToString()
    {
        return Modifiers.Count == 0
            ? Key.ToString()
            : string.Join("+", Modifiers.Select(m => m.ToString()).Append(Key.ToString()));
    }
}

public static class KeyboardShortcutParser
{
    public static bool TryParse(string? value, out KeyboardShortcut shortcut, out string? error)
    {
        shortcut = new KeyboardShortcut { Modifiers = [], Key = KeyboardKey.Space };
        error = null;

        if (string.IsNullOrWhiteSpace(value))
        {
            error = "Shortcut is empty.";
            return false;
        }

        var parts = value.Split('+', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
        {
            error = "Shortcut is empty.";
            return false;
        }

        var modifiers = new List<KeyboardModifier>();
        KeyboardKey? key = null;

        foreach (var part in parts)
        {
            if (TryParseModifier(part, out var modifier))
            {
                if (!modifiers.Contains(modifier))
                {
                    modifiers.Add(modifier);
                }

                continue;
            }

            if (key is not null)
            {
                error = "Shortcut can only contain one main key.";
                return false;
            }

            if (!TryParseKey(part, out var parsedKey))
            {
                error = $"Unsupported key: {part}";
                return false;
            }

            key = parsedKey;
        }

        if (key is null)
        {
            error = "Shortcut must contain a main key.";
            return false;
        }

        shortcut = new KeyboardShortcut
        {
            Modifiers = modifiers,
            Key = key.Value
        };
        return true;
    }

    private static bool TryParseModifier(string value, out KeyboardModifier modifier)
    {
        switch (value.Trim().ToLowerInvariant())
        {
            case "ctrl":
            case "control":
                modifier = KeyboardModifier.Control;
                return true;
            case "shift":
                modifier = KeyboardModifier.Shift;
                return true;
            case "alt":
                modifier = KeyboardModifier.Alt;
                return true;
            case "win":
            case "windows":
            case "meta":
                modifier = KeyboardModifier.Windows;
                return true;
            default:
                modifier = default;
                return false;
        }
    }

    private static bool TryParseKey(string value, out KeyboardKey key)
    {
        var normalized = value.Trim();
        if (normalized.Length == 1)
        {
            var c = char.ToUpperInvariant(normalized[0]);
            if (c is >= 'A' and <= 'Z')
            {
                key = Enum.Parse<KeyboardKey>(c.ToString());
                return true;
            }

            if (c is >= '0' and <= '9')
            {
                key = Enum.Parse<KeyboardKey>("D" + c);
                return true;
            }
        }

        switch (normalized.ToLowerInvariant())
        {
            case "space":
            case "spacebar":
                key = KeyboardKey.Space;
                return true;
            case "enter":
            case "return":
                key = KeyboardKey.Enter;
                return true;
            case "esc":
            case "escape":
                key = KeyboardKey.Escape;
                return true;
            default:
                return Enum.TryParse(normalized, ignoreCase: true, out key);
        }
    }
}
