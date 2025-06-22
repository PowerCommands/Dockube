using PainKiller.CommandPrompt.CoreLib.Modules.InfoPanelModule.Contracts;
using PainKiller.DockubeClient.Managers;

namespace PainKiller.DockubeClient.DomainObjects;
public class DockubeInfoPanelContent() : IInfoPanelContent
{
    public string GetText()
    {
        var sslVersion = GetSslVersion();
        var currentEnvironment = KubeEnvironmentManager.GetVersion();
        return $"SSL version: {sslVersion}\nCurrent Environment: {currentEnvironment}";
    }
    private string GetSslVersion()
    {
        var versionInfo = SslService.Default.GetVersion().Trim().Split(' ').Take(2);
        var retVal = $"{string.Join(' ', versionInfo).Trim()}";
        return retVal;
    }
}