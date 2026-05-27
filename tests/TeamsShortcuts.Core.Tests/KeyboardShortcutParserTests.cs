using TeamsShortcuts.Core;

namespace TeamsShortcuts.Core.Tests;

public sealed class KeyboardShortcutParserTests
{
    [Fact]
    public void ParsesCustomShortcutWithModifiers()
    {
        Assert.True(KeyboardShortcutParser.TryParse("Ctrl+Shift+K", out var shortcut, out var error), error);
        Assert.Equal([KeyboardModifier.Control, KeyboardModifier.Shift], shortcut.Modifiers);
        Assert.Equal(KeyboardKey.K, shortcut.Key);
    }

    [Fact]
    public void RejectsShortcutWithoutMainKey()
    {
        Assert.False(KeyboardShortcutParser.TryParse("Ctrl+Shift", out _, out var error));
        Assert.Equal("Shortcut must contain a main key.", error);
    }

    [Fact]
    public void RejectsTwoMainKeys()
    {
        Assert.False(KeyboardShortcutParser.TryParse("Ctrl+A+B", out _, out var error));
        Assert.Equal("Shortcut can only contain one main key.", error);
    }
}
