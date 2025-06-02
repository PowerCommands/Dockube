namespace PainKiller.CommandPrompt.CoreLib.Modules.InfoPanelModule.Contracts;
public interface IInfoPanelService
{
    void RegisterContent(IInfoPanel panel);
    void Start();
    void Stop();
    void Update();
}