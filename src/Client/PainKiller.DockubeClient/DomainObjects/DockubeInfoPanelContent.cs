using PainKiller.CommandPrompt.CoreLib.Modules.InfoPanelModule.Contracts;

namespace PainKiller.DockubeClient.DomainObjects;
public class DockubeInfoPanelContent(string executablePath) : IInfoPanelContent
{
    public string GetText()
    {
        ISslService sslService = new SslService(executablePath);
        var versionInfo = sslService.GetVersion().Trim().Split(' ').Take(2);
        var retVal = $"SSL version: {string.Join(' ', versionInfo).Trim()}";
        var shortText = retVal;
        return retVal.Length > Console.WindowWidth ? shortText : retVal;
    }
}