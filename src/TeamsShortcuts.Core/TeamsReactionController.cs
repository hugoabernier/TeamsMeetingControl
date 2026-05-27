using System.Diagnostics;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.UIA3;

namespace TeamsShortcuts.Core;

public sealed class TeamsReactionController(TeamsWindowFinder finder, ITeamsShortcutLogger log) : ITeamsReactionService
{
    private static readonly TimeSpan AttachTimeout = TimeSpan.FromSeconds(4);
    private static readonly TimeSpan ButtonSearchTimeout = TimeSpan.FromSeconds(5);

    private static readonly string[] ReactMenuButtonNames =
    [
        "React",
        "Reactions",
        "Send a reaction",
        "Raise hand and react",
        "Raise your hand and react",
        "More reactions"
    ];

    public async Task SendReactionAsync(TeamsReaction reaction, CancellationToken cancellationToken)
    {
        var selected = finder.FindBestTeamsMeetingWindow();
        if (selected is null)
        {
            throw new TeamsWindowNotFoundException();
        }

        var previous = Win32.GetForegroundWindowHandle();
        try
        {
            log.Verbose("Bringing Teams to foreground for reaction...");
            _ = Win32.BringToForeground(selected.Window.Hwnd);

            log.Verbose("Revealing Teams meeting toolbar with mouse movement...");
            _ = Win32.MoveMouseToBottomCenter(selected.Window.Hwnd);
            await Task.Delay(350, cancellationToken);

            using var automation = new UIA3Automation();
            var root = await AttachToWindow(automation, selected.Window.Hwnd, cancellationToken);
            if (root is null)
            {
                throw new InvalidOperationException("Could not attach UI Automation to the selected Teams window.");
            }

            log.Verbose("Searching for Teams reactions menu button...");
            var reactButton = await FindButtonByNames(root, ReactMenuButtonNames, ButtonSearchTimeout, maxElements: 2_000, maxDepth: 16, cancellationToken);
            if (reactButton is null)
            {
                throw new InvalidOperationException("Could not find the Teams React/Reactions menu button.");
            }

            log.Verbose($"Found reactions menu button: {Describe(reactButton)}");
            if (!InvokeOrClick(reactButton))
            {
                throw new InvalidOperationException("Found the Teams reactions menu button, but could not invoke or click it.");
            }

            await Task.Delay(500, cancellationToken);

            var reactionNames = ReactionNames(reaction);
            log.Verbose($"Searching for {reaction} reaction button...");
            var reactionButton = await FindButtonByNames(root, reactionNames, ButtonSearchTimeout, maxElements: 1_500, maxDepth: 14, cancellationToken);
            if (reactionButton is null)
            {
                log.Verbose("Reaction was not found under the Teams window. Searching desktop popups after menu open...");
                using var desktopAutomation = new UIA3Automation();
                var desktop = desktopAutomation.GetDesktop();
                reactionButton = await FindButtonByNames(desktop, reactionNames, ButtonSearchTimeout, maxElements: 2_000, maxDepth: 8, cancellationToken);
            }

            if (reactionButton is null)
            {
                throw new InvalidOperationException($"Could not find the {reaction} reaction button after opening the reactions menu.");
            }

            log.Verbose($"Found reaction button: {Describe(reactionButton)}");
            if (!InvokeOrClick(reactionButton))
            {
                throw new InvalidOperationException($"Found the {reaction} reaction button, but could not invoke or click it.");
            }
        }
        finally
        {
            if (previous != IntPtr.Zero)
            {
                _ = Win32.RestoreForegroundWindow(previous);
            }
        }
    }

    private async Task<AutomationElement?> AttachToWindow(UIA3Automation automation, IntPtr hwnd, CancellationToken token)
    {
        log.Verbose($"Attaching UIA to hwnd 0x{hwnd.ToInt64():X}...");
        return await RunWithTimeout("UIA attach", () => automation.FromHandle(hwnd), AttachTimeout, token);
    }

    private async Task<AutomationElement?> FindButtonByNames(
        AutomationElement root,
        IReadOnlyList<string> names,
        TimeSpan timeout,
        int maxElements,
        int maxDepth,
        CancellationToken token)
    {
        return await RunWithTimeout(
            "bounded UIA button search",
            () => SearchButtons(root, names, maxElements, maxDepth, token),
            timeout,
            token);
    }

    private static AutomationElement? SearchButtons(
        AutomationElement root,
        IReadOnlyList<string> names,
        int maxElements,
        int maxDepth,
        CancellationToken token)
    {
        var controlViewMatch = SearchButtonsWithChildren(
            root,
            names,
            maxElements,
            maxDepth,
            token,
            element => element.FindAllChildren());

        if (controlViewMatch is not null)
        {
            return controlViewMatch;
        }

        var rawWalker = root.Automation.TreeWalkerFactory.GetRawViewWalker();
        return SearchButtonsWithChildren(
            root,
            names,
            maxElements,
            maxDepth,
            token,
            element => GetRawChildren(rawWalker, element));
    }

    private static AutomationElement? SearchButtonsWithChildren(
        AutomationElement root,
        IReadOnlyList<string> names,
        int maxElements,
        int maxDepth,
        CancellationToken token,
        Func<AutomationElement, IReadOnlyList<AutomationElement>> getChildren)
    {
        var queue = new Queue<(AutomationElement Element, int Depth)>();
        var inspected = 0;
        AutomationElement? containsMatch = null;

        queue.Enqueue((root, 0));
        while (queue.Count > 0 && inspected < maxElements)
        {
            token.ThrowIfCancellationRequested();
            var (element, depth) = queue.Dequeue();
            inspected++;

            if (IsButton(element))
            {
                var name = SafeName(element);
                if (names.Any(candidate => string.Equals(name, candidate, StringComparison.OrdinalIgnoreCase)))
                {
                    return element;
                }

                if (containsMatch is null &&
                    names.Any(candidate => name.Contains(candidate, StringComparison.OrdinalIgnoreCase)))
                {
                    containsMatch = element;
                }
            }

            if (depth >= maxDepth)
            {
                continue;
            }

            IReadOnlyList<AutomationElement> children;
            try
            {
                children = getChildren(element);
            }
            catch
            {
                continue;
            }

            foreach (var child in children)
            {
                queue.Enqueue((child, depth + 1));
            }
        }

        return containsMatch;
    }

    private async Task<T?> RunWithTimeout<T>(
        string stepName,
        Func<T?> action,
        TimeSpan timeout,
        CancellationToken token)
        where T : class
    {
        var stopwatch = Stopwatch.StartNew();
        log.Verbose($"Starting {stepName} with {timeout.TotalSeconds:n0}s timeout...");
        var task = Task.Run(action, token);
        var delay = Task.Delay(timeout, token);
        var completed = await Task.WhenAny(task, delay);

        if (completed != task)
        {
            log.Warn($"{stepName} timed out after {stopwatch.ElapsedMilliseconds} ms.");
            return null;
        }

        var result = await task;
        stopwatch.Stop();
        log.Verbose($"{stepName} completed in {stopwatch.ElapsedMilliseconds} ms.");
        return result;
    }

    private bool InvokeOrClick(AutomationElement element)
    {
        try
        {
            element.AsButton().Invoke();
            log.Verbose("Invocation method: InvokePattern/Button.Invoke.");
            return true;
        }
        catch
        {
        }

        try
        {
            var rect = element.BoundingRectangle;
            if (rect.Width > 0 && rect.Height > 0)
            {
                var x = (int)Math.Round(Convert.ToDouble(rect.Left + (rect.Width / 2)));
                var y = (int)Math.Round(Convert.ToDouble(rect.Top + (rect.Height / 2)));
                _ = Win32.SetCursorPos(x, y);
                Win32.MouseLeftClick();
                log.Verbose("Invocation method: mouse click on bounding rectangle center.");
                return true;
            }
        }
        catch
        {
        }

        try
        {
            element.Focus();
            var keyboard = new Win32KeyboardInputService();
            keyboard.SendShortcutAsync(new KeyboardShortcut { Modifiers = [], Key = KeyboardKey.Enter }, CancellationToken.None).GetAwaiter().GetResult();
            log.Verbose("Invocation method: focus plus Enter.");
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static IReadOnlyList<AutomationElement> GetRawChildren(ITreeWalker walker, AutomationElement element)
    {
        var children = new List<AutomationElement>();
        var child = walker.GetFirstChild(element);

        while (child is not null)
        {
            children.Add(child);
            child = walker.GetNextSibling(child);
        }

        return children;
    }

    private static string[] ReactionNames(TeamsReaction reaction)
    {
        return reaction switch
        {
            TeamsReaction.Like => ["Like", "Thumbs up", "Send like reaction", "Send a like reaction"],
            TeamsReaction.Love => ["Love", "Heart", "Send love reaction", "Send a love reaction"],
            TeamsReaction.Applause => ["Applause", "Clap", "Clapping", "Send applause reaction", "Send an applause reaction"],
            TeamsReaction.Laugh => ["Laugh", "Laughing", "Send laugh reaction", "Send a laugh reaction"],
            TeamsReaction.Surprised => ["Surprised", "Surprise", "Wow", "Send surprised reaction", "Send a surprised reaction"],
            _ => throw new ArgumentOutOfRangeException(nameof(reaction), reaction, null)
        };
    }

    private static bool IsButton(AutomationElement element)
    {
        try { return element.ControlType == ControlType.Button; }
        catch { return false; }
    }

    private static string Describe(AutomationElement element)
    {
        return $"ControlType={SafeControlType(element)} Name=\"{SafeName(element)}\" AutomationId=\"{SafeAutomationId(element)}\" ClassName=\"{SafeClassName(element)}\" IsEnabled={SafeIsEnabled(element)} IsOffscreen={SafeIsOffscreen(element)} BoundingRectangle={SafeBoundingRectangle(element)}";
    }

    private static string SafeName(AutomationElement element)
    {
        try { return element.Name ?? string.Empty; }
        catch { return string.Empty; }
    }

    private static string SafeAutomationId(AutomationElement element)
    {
        try { return element.AutomationId ?? string.Empty; }
        catch { return string.Empty; }
    }

    private static string SafeClassName(AutomationElement element)
    {
        try { return element.ClassName ?? string.Empty; }
        catch { return string.Empty; }
    }

    private static string SafeControlType(AutomationElement element)
    {
        try { return element.ControlType.ToString(); }
        catch { return string.Empty; }
    }

    private static bool SafeIsEnabled(AutomationElement element)
    {
        try { return element.Properties.IsEnabled.ValueOrDefault; }
        catch { return false; }
    }

    private static bool SafeIsOffscreen(AutomationElement element)
    {
        try { return element.Properties.IsOffscreen.ValueOrDefault; }
        catch { return false; }
    }

    private static string SafeBoundingRectangle(AutomationElement element)
    {
        try { return element.BoundingRectangle.ToString(); }
        catch { return string.Empty; }
    }
}
