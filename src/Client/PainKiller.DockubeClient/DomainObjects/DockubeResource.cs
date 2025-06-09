namespace PainKiller.DockubeClient.DomainObjects;
public class DockubeResource
{
    public string Path { get; set; } = string.Empty;
    public string[] Before { get; set; } = [];
    public string[] After { get; set; } = [];
}