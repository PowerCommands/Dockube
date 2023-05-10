namespace DockubeCommands.DomainObjects;
public class TreeItem
{
    public string Path { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public List<TreeItem> Children { get; set; } = new();
}