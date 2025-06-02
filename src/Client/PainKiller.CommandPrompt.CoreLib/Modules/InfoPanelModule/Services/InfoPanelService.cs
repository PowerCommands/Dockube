using PainKiller.CommandPrompt.CoreLib.Configuration.Services;
using PainKiller.CommandPrompt.CoreLib.Core.Events;
using PainKiller.CommandPrompt.CoreLib.Modules.InfoPanelModule.Contracts;

namespace PainKiller.CommandPrompt.CoreLib.Modules.InfoPanelModule.Services;
public sealed class InfoPanelService : IInfoPanelService
{
    private bool _enabled;
    private int _margin;
    private int _updateIntervalInSeconds;
    private CancellationTokenSource? _cancellationTokenSource;

    // ReSharper disable once InconsistentNaming
    private static readonly Lazy<InfoPanelService> _instance = new(() => new InfoPanelService());
    public static InfoPanelService Instance => _instance.Value;
    private IInfoPanel? _infoPanel;

    private InfoPanelService() { }
    public void RegisterContent(IInfoPanel panel)
    {
        var configuration = ConfigurationService.Service.GetFlexible<ApplicationConfiguration>(Path.Combine(AppContext.BaseDirectory, "CommandPromptConfiguration.yaml"));
        _enabled = configuration.Configuration.Core.Modules.InfoPanel.Enabled;
        _margin = configuration.Configuration.Core.Modules.InfoPanel.Height;
        _updateIntervalInSeconds = configuration.Configuration.Core.Modules.InfoPanel.UpdateIntervalSeconds;
        _infoPanel = panel;
        ConsoleService.Writer.SetMargin(_margin);
        Console.CursorTop = _margin+1;
        EventBusService.Service.Subscribe<AfterCommandExecutionEvent>(eventData => Update());
        Console.CursorTop = 10;
        Start();
    }
    public void Start()
    {
        if (!_enabled || _infoPanel == null) return;

        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource = new CancellationTokenSource();
        _infoPanel.Draw(_margin);

        if (_updateIntervalInSeconds > 0)
        {
            StartAutoUpdate();
        }
    }
    public void Stop()
    {
        _cancellationTokenSource?.Cancel();
    }
    public void Update()
    {
        if (_infoPanel != null && _enabled)
        {
            _infoPanel.Draw(_margin);
        }
    }
    private void StartAutoUpdate()
    {
        if (_cancellationTokenSource != null)
            Task.Run(async () =>
            {
                while (_cancellationTokenSource is { Token.IsCancellationRequested: false })
                {
                    await Task.Delay(_updateIntervalInSeconds * 1000);

                    if (_infoPanel != null)
                    {
                        _infoPanel.Draw(_margin);
                    }
                }
            }, _cancellationTokenSource.Token);
    }
}