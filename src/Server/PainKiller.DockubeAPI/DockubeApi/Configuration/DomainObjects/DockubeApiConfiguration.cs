using DockubeApi.Configuration.DomainObjects;
namespace PainKiller.DockubeClient.Configuration;
public class DockubeApiConfiguration : ApplicationConfiguration
{
    public string ManifestBasePath { get; set; } = "";
    public GitlabConfiguration Gitlab { get; set; } = new();
}