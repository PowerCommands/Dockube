using PainKiller.CommandPrompt.CoreLib.Modules.InfoPanelModule.Contracts;
namespace PainKiller.CommandPrompt.CoreLib.Modules.InfoPanelModule.DomainObjects;
public class DefaultInfoPanelContent : IInfoPanelContent
{
    public string GetText()
    {
        var retVal = $"{DateTime.Now.ToString("dddd d MMMM yyyy HH:mm:ss", System.Globalization.CultureInfo.CurrentCulture)}";
        var shortText = $"{DateTime.Now.ToString("dddd d MMMM", System.Globalization.CultureInfo.CurrentCulture)}";

        if (retVal.Length > Console.WindowWidth) return shortText;
        return retVal;
    }
}