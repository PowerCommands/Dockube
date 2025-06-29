using PainKiller.CommandPrompt.CoreLib.Modules.InfoPanelModule.Contracts;
using PainKiller.DockubeClient.Managers;

namespace PainKiller.DockubeClient.DomainObjects;
public class DockubeInfoPanelContent(DockubeConfiguration configuration) : IInfoPanelContent
{
    public string GetText()
    {
        var sslVersion = GetSslVersion();
        var currentEnvironment = KubeEnvironmentManager.GetVersion();

        var statuses = string.Join(", ", ServiceStatusManager.GetServicesStatus(configuration).Select(s => $"{s.Name} {(s.IsAvailable ? "OK" : "OFFLINE")}"));
        return $"SSL version: {sslVersion}\t\tServices: {statuses}\nCurrent Environment: {currentEnvironment}";
    }
    private string GetSslVersion()
    {
        var versionInfo = SslService.Default.GetVersion().Trim().Split(' ').Take(2);
        var retVal = $"{string.Join(' ', versionInfo).Trim()}";
        return retVal;
    }
}