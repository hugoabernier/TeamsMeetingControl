using TeamsShortcuts.Core;

namespace TeamsShortcuts.StreamDeck;

internal static class TeamsServices
{
    public static ITeamsCommandService Create(ITeamsShortcutLogger? log = null)
    {
        var logger = log ?? NullTeamsShortcutLogger.Instance;
        var finder = new TeamsWindowFinder(logger);
        var focus = new Win32WindowFocusService(finder, logger);
        var keyboard = new Win32KeyboardInputService();
        var reactions = new TeamsReactionController(finder, logger);
        return new TeamsCommandService(keyboard, focus, reactions, logger);
    }
}
