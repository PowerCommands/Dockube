namespace DockubeApi.Configuration.DomainObjects;
public class YamlContainer<T> where T : new()
{
    public string Version { get; set; } = "1.0";
    public T Configuration { get; set; } = new();
}