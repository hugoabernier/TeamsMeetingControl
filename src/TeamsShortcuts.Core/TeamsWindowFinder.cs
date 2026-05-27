namespace TeamsShortcuts.Core;

public sealed class TeamsWindowFinder(ITeamsShortcutLogger log)
{
    public IReadOnlyList<TeamsWindowCandidate> FindCandidates()
    {
        using var _ = log.TimedVerbose("Win32 top-level window enumeration");

        var windows = Win32.GetTopLevelWindows();
        log.Verbose($"Top-level visible windows: {windows.Count}");

        return windows
            .Where(IsTeamsLikeCandidate)
            .Select(w => new TeamsWindowCandidate(w, Score(w)))
            .OrderByDescending(c => c.Score)
            .ToArray();
    }

    public TeamsWindowCandidate? FindBestTeamsMeetingWindow()
    {
        return ChooseBestTeamsMeetingWindow(FindCandidates());
    }

    public TeamsWindowCandidate? ChooseBestTeamsMeetingWindow(IEnumerable<TeamsWindowCandidate> candidates)
    {
        return candidates.FirstOrDefault(candidate => IsDesktopTeamsHost(candidate.Window));
    }

    public void PrintDiagnoseCandidates(IReadOnlyList<TeamsWindowCandidate> candidates, bool verbose)
    {
        log.Info($"Teams-like top-level windows: {candidates.Count}");

        foreach (var candidate in candidates)
        {
            log.Info(verbose
                ? Describe(candidate)
                : $"  {candidate.Window.HwndHex} score={candidate.Score} title=\"{candidate.Window.Title}\" process={candidate.Window.ProcessName}");
        }
    }

    public static string Describe(TeamsWindowCandidate candidate)
    {
        var window = candidate.Window;
        return $"  hwnd={window.HwndHex} score={candidate.Score} title=\"{window.Title}\" class=\"{window.ClassName}\" pid={window.ProcessId} process=\"{window.ProcessName}\" rect={window.Rect}";
    }

    private static bool IsTeamsLikeCandidate(Win32Window window)
    {
        return ContainsAny(window.Title, "Teams", "Microsoft Teams", "Meeting", "Call")
            || IsTeamsProcess(window.ProcessName)
            || ContainsAny(window.ClassName, "Teams");
    }

    private static int Score(Win32Window window)
    {
        var score = 0;

        if (ContainsAny(window.Title, "Microsoft Teams"))
        {
            score += 50;
        }

        if (ContainsAny(window.Title, "Teams"))
        {
            score += 25;
        }

        if (ContainsAny(window.Title, "Meeting", "Call"))
        {
            score += 35;
        }

        if (IsTeamsProcess(window.ProcessName))
        {
            score += 45;
        }

        if (ContainsAny(window.ClassName, "Teams"))
        {
            score += 10;
        }

        if (string.IsNullOrWhiteSpace(window.Title))
        {
            score -= 25;
        }

        if (window.Rect.Width < 500 || window.Rect.Height < 300)
        {
            score -= 30;
        }

        if (window.Rect.Width * window.Rect.Height < 250_000)
        {
            score -= 20;
        }

        return score;
    }

    private static bool IsDesktopTeamsHost(Win32Window window)
    {
        return IsTeamsProcess(window.ProcessName)
            || ContainsAny(window.ClassName, "TeamsWebView", "Teams");
    }

    private static bool IsTeamsProcess(string processName)
    {
        return string.Equals(processName, "ms-teams", StringComparison.OrdinalIgnoreCase)
            || string.Equals(processName, "msteams", StringComparison.OrdinalIgnoreCase)
            || string.Equals(processName, "Teams", StringComparison.OrdinalIgnoreCase);
    }

    private static bool ContainsAny(string value, params string[] needles)
    {
        return needles.Any(needle => value.Contains(needle, StringComparison.OrdinalIgnoreCase));
    }
}

public sealed record TeamsWindowCandidate(Win32Window Window, int Score);
