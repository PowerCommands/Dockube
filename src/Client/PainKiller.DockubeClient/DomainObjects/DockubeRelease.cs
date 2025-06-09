namespace PainKiller.DockubeClient.DomainObjects;

public class DockubeRelease
{
    public string Name { get; set; } = string.Empty;
    public string Namespace { get; set; } = "default";
    public List<DockubeResource> Resources { get; set; } = [];
}