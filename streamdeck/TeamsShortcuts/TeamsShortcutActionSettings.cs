using Newtonsoft.Json;
using TeamsShortcuts.Core;

namespace TeamsShortcuts.StreamDeck;

internal sealed class TeamsShortcutActionSettings
{
    [JsonProperty("focusMode")]
    public string FocusMode { get; set; } = TeamsFocusMode.FocusTeams.ToString();

    [JsonProperty("delayAfterFocusMilliseconds")]
    public int DelayAfterFocusMilliseconds { get; set; } = 100;

    public TeamsCommandOptions ToOptions()
    {
        var focusMode = Enum.TryParse<TeamsFocusMode>(FocusMode, ignoreCase: true, out var parsed)
            ? parsed
            : TeamsFocusMode.FocusTeams;

        return new TeamsCommandOptions
        {
            FocusMode = focusMode,
            DelayAfterFocusMilliseconds = Math.Clamp(DelayAfterFocusMilliseconds, 0, 2_000)
        };
    }
}
