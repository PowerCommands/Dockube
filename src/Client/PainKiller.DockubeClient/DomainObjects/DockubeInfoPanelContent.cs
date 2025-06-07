using PainKiller.CommandPrompt.CoreLib.Modules.InfoPanelModule.Contracts;

namespace PainKiller.DockubeClient.DomainObjects;
public class DockubeInfoPanelContent(string executablePath) : IInfoPanelContent
{
    public string GetText()
    {
        var sslVersion = GetSslVersion();
        return $"SSL version: {sslVersion}";
    }
    private string GetSslVersion()
    {
        ISslService sslService = new SslService(executablePath);
        var versionInfo = sslService.GetVersion().Trim().Split(' ').Take(2);
        var retVal = $"{string.Join(' ', versionInfo).Trim()}";
        return retVal;
    }
}