using PainKiller.CommandPrompt.CoreLib.Core.Enums;
namespace PainKiller.CommandPrompt.CoreLib.Core.DomainObjects;
public class ToolbarConfiguration
{
    public HideToolbarOption HideToolbarOption { get; set; } = HideToolbarOption.Never;
    public List<ToolbarItemConfiguration> ToolbarItems { get; set; } = new();
}