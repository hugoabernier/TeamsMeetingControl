using TeamsShortcuts.Core;

namespace TeamsShortcuts.ConsoleApp;

internal static class Program
{
    private const int ExitSuccess = 0;
    private const int ExitInvalidArguments = 1;
    private const int ExitTeamsNotFound = 2;
    private const int ExitActionFailed = 3;
    private const int ExitUnexpectedException = 4;

    public static async Task<int> Main(string[] args)
    {
        if (args.Length > 0 && args[0].Equals("diagnose", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("TeamsMeetingControl diagnose starting...");
            Console.Out.Flush();
        }

        var options = CliOptions.Parse(args);
        if (!options.IsValid)
        {
            Console.Error.WriteLine(options.ErrorMessage);
            PrintHelp();
            return ExitInvalidArguments;
        }

        var log = new ConsoleTeamsShortcutLogger(options.Verbose);

        if (!OperatingSystem.IsWindows())
        {
            log.Error("TeamsMeetingControl is Windows-only and must run on Windows.");
            return ExitUnexpectedException;
        }

        try
        {
            var finder = new TeamsWindowFinder(log);
            var focus = new Win32WindowFocusService(finder, log);
            var keyboard = new Win32KeyboardInputService();
            var reactions = new TeamsReactionController(finder, log);
            var commandService = new TeamsCommandService(keyboard, focus, reactions, log);
            var diagnostics = new TeamsDiagnosticsService(finder, log);

            using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(25));
            var commandOptions = new TeamsCommandOptions
            {
                FocusMode = TeamsFocusMode.FocusTeamsAndRestorePrevious,
                DelayAfterFocusMilliseconds = 100
            };

            return options.Command switch
            {
                CliCommand.Diagnose => await diagnostics.DiagnoseAsync(options.Verbose, options.Deep, cancellation.Token) ? ExitSuccess : ExitTeamsNotFound,
                CliCommand.Hand => await SendCommand(commandService, TeamsCommand.ToggleHand, commandOptions, cancellation.Token),
                CliCommand.React => await SendCommand(commandService, ReactionToCommand(options.Reaction!.Value), commandOptions, cancellation.Token),
                CliCommand.Help => ExitSuccess,
                _ => ExitInvalidArguments
            };
        }
        catch (TeamsWindowNotFoundException)
        {
            log.Error("Microsoft Teams meeting window was not found.");
            log.Error("Next step: start or join a Teams meeting, then run diagnose --verbose.");
            return ExitTeamsNotFound;
        }
        catch (OperationCanceledException)
        {
            log.Error("Timed out before the Teams action completed.");
            log.Error("Next step: run diagnose --verbose --deep while the Teams meeting toolbar is visible.");
            return ExitActionFailed;
        }
        catch (Exception ex)
        {
            log.Error("Unexpected error:");
            log.Error(ex.ToString());
            return ExitUnexpectedException;
        }
    }

    private static async Task<int> SendCommand(ITeamsCommandService service, TeamsCommand command, TeamsCommandOptions options, CancellationToken token)
    {
        await service.SendCommandAsync(command, options, token);
        Console.WriteLine($"Sent Teams command: {command}.");
        return ExitSuccess;
    }

    private static TeamsCommand ReactionToCommand(TeamsReaction reaction)
    {
        return reaction switch
        {
            TeamsReaction.Like => TeamsCommand.ReactLike,
            TeamsReaction.Love => TeamsCommand.ReactLove,
            TeamsReaction.Applause => TeamsCommand.ReactApplause,
            TeamsReaction.Laugh => TeamsCommand.ReactLaugh,
            TeamsReaction.Surprised => TeamsCommand.ReactSurprised,
            _ => throw new ArgumentOutOfRangeException(nameof(reaction), reaction, null)
        };
    }

    private static void PrintHelp()
    {
        Console.WriteLine("""
TeamsMeetingControl
Windows-only Microsoft Teams keyboard controls proof of concept.

Usage:
  TeamsMeetingControl.exe diagnose
  TeamsMeetingControl.exe diagnose --verbose
  TeamsMeetingControl.exe diagnose --verbose --deep
  TeamsMeetingControl.exe hand
  TeamsMeetingControl.exe react like
  TeamsMeetingControl.exe react love
  TeamsMeetingControl.exe react applause
  TeamsMeetingControl.exe react laugh
  TeamsMeetingControl.exe react surprised
""");
    }

    private sealed record CliOptions(
        bool IsValid,
        CliCommand Command,
        TeamsReaction? Reaction,
        bool Verbose,
        bool Deep,
        string? ErrorMessage)
    {
        public static CliOptions Parse(string[] args)
        {
            if (args.Length == 0 ||
                args.Any(a => a.Equals("--help", StringComparison.OrdinalIgnoreCase) ||
                              a.Equals("-h", StringComparison.OrdinalIgnoreCase)))
            {
                PrintHelp();
                return new CliOptions(true, CliCommand.Help, null, false, false, null);
            }

            var verbose = false;
            var deep = false;
            var positional = new List<string>();

            foreach (var arg in args)
            {
                if (arg.Equals("--verbose", StringComparison.OrdinalIgnoreCase) ||
                    arg.Equals("-v", StringComparison.OrdinalIgnoreCase))
                {
                    verbose = true;
                }
                else if (arg.Equals("--deep", StringComparison.OrdinalIgnoreCase))
                {
                    deep = true;
                }
                else if (arg.StartsWith("-", StringComparison.Ordinal))
                {
                    return new CliOptions(false, CliCommand.Help, null, verbose, deep, $"Unknown option: {arg}");
                }
                else
                {
                    positional.Add(arg);
                }
            }

            if (positional.Count == 0)
            {
                return new CliOptions(false, CliCommand.Help, null, verbose, deep, "Missing command.");
            }

            switch (positional[0].ToLowerInvariant())
            {
                case "diagnose":
                case "diag":
                    return new CliOptions(true, CliCommand.Diagnose, null, verbose, deep, null);
                case "hand":
                    return new CliOptions(true, CliCommand.Hand, null, verbose, deep, null);
                case "react":
                    if (positional.Count < 2)
                    {
                        return new CliOptions(false, CliCommand.React, null, verbose, deep, "Missing reaction name.");
                    }

                    if (!TryParseReaction(positional[1], out var reaction))
                    {
                        return new CliOptions(false, CliCommand.React, null, verbose, deep, $"Unknown reaction: {positional[1]}");
                    }

                    return new CliOptions(true, CliCommand.React, reaction, verbose, deep, null);
                default:
                    return new CliOptions(false, CliCommand.Help, null, verbose, deep, $"Unknown command: {positional[0]}");
            }
        }

        private static bool TryParseReaction(string value, out TeamsReaction reaction)
        {
            switch (value.Trim().ToLowerInvariant())
            {
                case "like":
                    reaction = TeamsReaction.Like;
                    return true;
                case "love":
                case "heart":
                    reaction = TeamsReaction.Love;
                    return true;
                case "applause":
                case "applaud":
                case "clap":
                    reaction = TeamsReaction.Applause;
                    return true;
                case "laugh":
                    reaction = TeamsReaction.Laugh;
                    return true;
                case "surprised":
                case "surprise":
                case "wow":
                    reaction = TeamsReaction.Surprised;
                    return true;
                default:
                    reaction = default;
                    return false;
            }
        }
    }
}

internal enum CliCommand
{
    Help,
    Diagnose,
    Hand,
    React
}
