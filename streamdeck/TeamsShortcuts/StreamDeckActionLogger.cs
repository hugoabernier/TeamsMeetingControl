using BarRaider.SdTools;
using TeamsShortcuts.Core;

namespace TeamsShortcuts.StreamDeck;

internal sealed class StreamDeckActionLogger(string actionName) : ITeamsShortcutLogger
{
    public bool IsVerbose => true;

    public void Info(string message) => Logger.Instance.LogMessage(TracingLevel.INFO, $"[{actionName}] {message}");
    public void Verbose(string message) => Logger.Instance.LogMessage(TracingLevel.DEBUG, $"[{actionName}] {message}");
    public void Warn(string message) => Logger.Instance.LogMessage(TracingLevel.WARN, $"[{actionName}] {message}");
    public void Error(string message) => Logger.Instance.LogMessage(TracingLevel.ERROR, $"[{actionName}] {message}");
    public IDisposable TimedVerbose(string name) => NullTeamsShortcutLogger.Instance.TimedVerbose(name);
}
