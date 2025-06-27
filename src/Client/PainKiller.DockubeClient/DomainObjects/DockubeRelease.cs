namespace PainKiller.DockubeClient.DomainObjects;

public class DockubeRelease
{
    public bool IsCore { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Namespace { get; set; } = "default";
    public string Node { get; set; } = "docker-desktop";
    public ResourceProfile ResourceProfile { get; set; } = new();
    public List<DockubeResource> Resources { get; set; } = [];
    public int Retries { get; set; } = 200;
}