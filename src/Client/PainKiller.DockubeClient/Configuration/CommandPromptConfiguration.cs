using PainKiller.CommandPrompt.CoreLib.Configuration.DomainObjects;
namespace PainKiller.DockubeClient.Configuration;
public class CommandPromptConfiguration : ApplicationConfiguration
{
    public DockubeConfiguration Dockube { get; set; } = new();
}