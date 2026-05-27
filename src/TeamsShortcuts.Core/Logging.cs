using System.Diagnostics;

namespace TeamsShortcuts.Core;

public interface ITeamsShortcutLogger
{
    bool IsVerbose { get; }
    void Info(string message);
    void Verbose(string message);
    void Warn(string message);
    void Error(string message);
    IDisposable TimedVerbose(string name);
}

public sealed class ConsoleTeamsShortcutLogger(bool verbose) : ITeamsShortcutLogger
{
    public bool IsVerbose { get; } = verbose;

    public void Info(string message)
    {
        Console.WriteLine(message);
        Console.Out.Flush();
    }

    public void Verbose(string message)
    {
        if (!IsVerbose)
        {
            return;
        }

        Console.WriteLine(message);
        Console.Out.Flush();
    }

    public void Warn(string message)
    {
        Console.WriteLine("WARNING: " + message);
        Console.Out.Flush();
    }

    public void Error(string message)
    {
        Console.Error.WriteLine(message);
        Console.Error.Flush();
    }

    public IDisposable TimedVerbose(string name)
    {
        return new TimedLogScope(this, name);
    }

    private sealed class TimedLogScope(ITeamsShortcutLogger log, string name) : IDisposable
    {
        private readonly Stopwatch _stopwatch = Stopwatch.StartNew();

        public void Dispose()
        {
            log.Verbose($"{name} completed in {_stopwatch.ElapsedMilliseconds} ms.");
        }
    }
}

public sealed class NullTeamsShortcutLogger : ITeamsShortcutLogger
{
    public static NullTeamsShortcutLogger Instance { get; } = new();
    public bool IsVerbose => false;
    public void Info(string message) { }
    public void Verbose(string message) { }
    public void Warn(string message) { }
    public void Error(string message) { }
    public IDisposable TimedVerbose(string name) => NoopDisposable.Instance;

    private sealed class NoopDisposable : IDisposable
    {
        public static NoopDisposable Instance { get; } = new();
        public void Dispose() { }
    }
}
