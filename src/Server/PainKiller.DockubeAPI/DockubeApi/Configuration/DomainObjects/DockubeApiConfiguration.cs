namespace DockubeApi.Configuration.DomainObjects;
public class DockubeApiConfiguration : ApplicationConfiguration
{
    public string ManifestBasePath { get; set; } = "";
    public GitlabConfiguration Gitlab { get; set; } = new();    
}