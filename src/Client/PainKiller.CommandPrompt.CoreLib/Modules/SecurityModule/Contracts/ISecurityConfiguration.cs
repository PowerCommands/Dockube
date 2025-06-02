using PainKiller.CommandPrompt.CoreLib.Modules.SecurityModule.Configuration;

namespace PainKiller.CommandPrompt.CoreLib.Modules.SecurityModule.Contracts;

public interface ISecurityConfiguration
{
    public SecurityConfiguration Security { get; set; }
}