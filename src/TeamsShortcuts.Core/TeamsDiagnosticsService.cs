using System.Runtime.InteropServices;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;

namespace TeamsShortcuts.Core;

public sealed class TeamsDiagnosticsService(TeamsWindowFinder finder, ITeamsShortcutLogger log)
{
    private static readonly TimeSpan AttachTimeout = TimeSpan.FromSeconds(4);
    private static readonly TimeSpan DeepDiagnoseTimeout = TimeSpan.FromSeconds(10);

    public async Task<bool> DiagnoseAsync(bool verbose, bool deep, CancellationToken token)
    {
        log.Info($"Process architecture: {(Environment.Is64BitProcess ? "x64" : "x86")} / {RuntimeInformation.ProcessArchitecture}");
        log.Info($"OS: {RuntimeInformation.OSDescription}");
        log.Info("Enumerating top-level windows with Win32 first...");

        var candidates = finder.FindCandidates();
        finder.PrintDiagnoseCandidates(candidates, verbose);

        var selected = finder.ChooseBestTeamsMeetingWindow(candidates);
        if (selected is null)
        {
            log.Error("Microsoft Teams meeting window was not found.");
            log.Error("Next step: start or join a Teams meeting, then run diagnose --verbose again.");
            return false;
        }

        log.Info("Selected Teams window:");
        log.Info(TeamsWindowFinder.Describe(selected));

        if (!deep)
        {
            log.Info("Deep UI Automation scan skipped. Pass --deep to enumerate a capped set of controls.");
            return true;
        }

        log.Info("Deep UI Automation scan requested. Attaching only to the selected Teams hwnd...");
        using var automation = new UIA3Automation();
        var root = await RunWithTimeout("UIA attach", () => automation.FromHandle(selected.Window.Hwnd), AttachTimeout, token);
        if (root is null)
        {
            log.Error("Could not attach UI Automation to the selected Teams window.");
            log.Error("Next step: run this utility at the same privilege level as Teams.");
            return false;
        }

        var controls = await RunWithTimeout(
            "deep UIA control enumeration",
            () => EnumerateControls(root, maxControls: 300, token),
            DeepDiagnoseTimeout,
            token);

        if (controls is null)
        {
            log.Error("Deep UI Automation enumeration timed out.");
            log.Error("Next step: make the Teams meeting toolbar visible, then run diagnose --verbose --deep again.");
            return false;
        }

        log.Info($"UIA controls shown: {controls.Count}");
        foreach (var control in controls)
        {
            log.Info(control);
        }

        if (controls.Count >= 300)
        {
            log.Warn("UIA control cap reached at 300 controls.");
        }

        return true;
    }

    private async Task<T?> RunWithTimeout<T>(string stepName, Func<T?> action, TimeSpan timeout, CancellationToken token)
        where T : class
    {
        log.Verbose($"Starting {stepName} with {timeout.TotalSeconds:n0}s timeout...");
        var task = Task.Run(action, token);
        var delay = Task.Delay(timeout, token);
        var completed = await Task.WhenAny(task, delay);

        if (completed != task)
        {
            log.Warn($"{stepName} timed out.");
            return null;
        }

        return await task;
    }

    private static List<string> EnumerateControls(AutomationElement root, int maxControls, CancellationToken token)
    {
        var controls = new List<string>(capacity: maxControls);
        var rawWalker = root.Automation.TreeWalkerFactory.GetRawViewWalker();
        var queue = new Queue<(AutomationElement Element, int Depth)>();
        queue.Enqueue((root, 0));

        while (queue.Count > 0 && controls.Count < maxControls)
        {
            token.ThrowIfCancellationRequested();
            var (element, depth) = queue.Dequeue();
            controls.Add(Describe(element));

            if (depth >= 10)
            {
                continue;
            }

            var child = rawWalker.GetFirstChild(element);
            while (child is not null)
            {
                queue.Enqueue((child, depth + 1));
                child = rawWalker.GetNextSibling(child);
            }
        }

        return controls;
    }

    private static string Describe(AutomationElement element)
    {
        return $"ControlType={Safe(() => element.ControlType.ToString())} Name=\"{Safe(() => element.Name ?? string.Empty)}\" AutomationId=\"{Safe(() => element.AutomationId ?? string.Empty)}\" ClassName=\"{Safe(() => element.ClassName ?? string.Empty)}\" IsEnabled={Safe(() => element.Properties.IsEnabled.ValueOrDefault.ToString())} IsOffscreen={Safe(() => element.Properties.IsOffscreen.ValueOrDefault.ToString())} BoundingRectangle={Safe(() => element.BoundingRectangle.ToString())}";
    }

    private static string Safe(Func<string> get)
    {
        try { return get(); }
        catch { return string.Empty; }
    }
}
