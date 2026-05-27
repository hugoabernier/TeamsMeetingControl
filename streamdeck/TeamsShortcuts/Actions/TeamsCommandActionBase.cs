using BarRaider.SdTools;
using Newtonsoft.Json.Linq;
using TeamsShortcuts.Core;

namespace TeamsShortcuts.StreamDeck.Actions;

internal abstract class TeamsCommandActionBase : KeypadBase
{
    private readonly string _normalTitle;
    private readonly ITeamsShortcutLogger _log;
    private readonly ITeamsCommandService _teams;
    protected TeamsShortcutActionSettings Settings { get; private set; } = new();

    protected TeamsCommandActionBase(SDConnection connection, InitialPayload payload, string normalTitle)
        : base(connection, payload)
    {
        _normalTitle = normalTitle;
        _log = new StreamDeckActionLogger(GetType().Name);
        _teams = TeamsServices.Create(_log);
        LoadSettings(payload.Settings);
        _ = Connection.SetTitleAsync(_normalTitle);
    }

    protected abstract TeamsCommand Command { get; }

    public override void KeyPressed(KeyPayload payload)
    {
        _ = ExecuteAsync(payload);
    }

    public override void KeyReleased(KeyPayload payload)
    {
    }

    public override void ReceivedSettings(ReceivedSettingsPayload payload)
    {
        LoadSettings(payload.Settings);
    }

    public override void ReceivedGlobalSettings(ReceivedGlobalSettingsPayload payload)
    {
    }

    public override void OnTick()
    {
    }

    public override void Dispose()
    {
    }

    protected async Task ExecuteCommandAsync(TeamsCommand command, string normalTitle, CancellationToken cancellationToken)
    {
        try
        {
            _log.Info($"Action pressed. Command={command}; FocusMode={Settings.FocusMode}");
            await _teams.SendCommandAsync(command, Settings.ToOptions(), cancellationToken);
            await FlashTitleAsync("Sent", normalTitle);
        }
        catch (TeamsWindowNotFoundException)
        {
            _log.Warn("Teams not found.");
            await FlashTitleAsync("Teams\nNot Found", normalTitle);
        }
        catch (Exception ex)
        {
            _log.Error(ex.ToString());
            await FlashTitleAsync("Error", normalTitle);
        }
    }

    protected Task FlashTitleAsync(string title, string normalTitle)
    {
        return Task.Run(async () =>
        {
            await Connection.SetTitleAsync(title);
            await Task.Delay(900);
            await Connection.SetTitleAsync(normalTitle);
        });
    }

    private async Task ExecuteAsync(KeyPayload payload)
    {
        using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        await ExecuteCommandAsync(Command, _normalTitle, cancellation.Token);
    }

    private void LoadSettings(JObject? settings)
    {
        Settings = settings?.ToObject<TeamsShortcutActionSettings>() ?? new TeamsShortcutActionSettings();
    }
}
