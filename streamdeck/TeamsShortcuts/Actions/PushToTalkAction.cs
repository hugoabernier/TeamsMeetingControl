using BarRaider.SdTools;
using TeamsShortcuts.Core;

namespace TeamsShortcuts.StreamDeck.Actions;

[PluginActionId(PluginIds.PushToTalk)]
internal sealed class PushToTalkAction : KeypadBase
{
    private readonly ITeamsShortcutLogger _log;
    private readonly ITeamsCommandService _teams;
    private TeamsShortcutActionSettings _settings = new();
    private bool _isDown;

    public PushToTalkAction(SDConnection connection, InitialPayload payload)
        : base(connection, payload)
    {
        _log = new StreamDeckActionLogger(nameof(PushToTalkAction));
        _teams = TeamsServices.Create(_log);
        _settings = payload.Settings?.ToObject<TeamsShortcutActionSettings>() ?? new TeamsShortcutActionSettings();
        _ = Connection.SetTitleAsync("Hold\nTalk");
    }

    public override void KeyPressed(KeyPayload payload)
    {
        if (_isDown)
        {
            return;
        }

        _isDown = true;
        _ = RunAsync(async token => await _teams.BeginTemporaryUnmuteAsync(_settings.ToOptions(), token));
    }

    public override void KeyReleased(KeyPayload payload)
    {
        _isDown = false;
        _ = RunAsync(async token => await _teams.EndTemporaryUnmuteAsync(_settings.ToOptions(), token));
    }

    public override void ReceivedSettings(ReceivedSettingsPayload payload)
    {
        _settings = payload.Settings?.ToObject<TeamsShortcutActionSettings>() ?? new TeamsShortcutActionSettings();
    }

    public override void ReceivedGlobalSettings(ReceivedGlobalSettingsPayload payload) { }
    public override void OnTick() { }

    public override void Dispose()
    {
        if (_isDown)
        {
            _isDown = false;
            try
            {
                _teams.EndTemporaryUnmuteAsync(_settings.ToOptions(), CancellationToken.None).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
            }
        }
    }

    private async Task RunAsync(Func<CancellationToken, Task> action)
    {
        try
        {
            using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            await action(cancellation.Token);
        }
        catch (TeamsWindowNotFoundException)
        {
            await Connection.SetTitleAsync("Teams\nNot Found");
        }
        catch (Exception ex)
        {
            _log.Error(ex.ToString());
            await Connection.SetTitleAsync("Error");
        }
    }
}
