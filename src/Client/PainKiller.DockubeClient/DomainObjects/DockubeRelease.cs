namespace PainKiller.DockubeClient.DomainObjects;

public class DockubeRelease
{
    public bool IsCore { get; set; }
    public int Order { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Namespace { get; set; } = "default";
    public List<DockubeResource> Resources { get; set; } = [];
}