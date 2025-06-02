using PainKiller.CommandPrompt.CoreLib.Modules.InfoPanelModule.Configuration;
using PainKiller.CommandPrompt.CoreLib.Modules.SecurityModule.Configuration;
using PainKiller.CommandPrompt.CoreLib.Modules.StorageModule.Configuration;

namespace PainKiller.CommandPrompt.CoreLib.Configuration.DomainObjects;
public class ModulesConfiguration
{
    public SecurityConfiguration Security { get; set; } = new();
    public StorageConfiguration Storage { get; set; } = new();
    public InfoPanelConfiguration InfoPanel { get; set; } = new();
}
