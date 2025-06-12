namespace PainKiller.DockubeClient.Configuration;
public class DockubeConfiguration
{
    public string ManifestBasePath { get; set; } = "Manifests";
    public DockubeRelease[] Releases { get; set; } = [];
    public SslConfiguration Ssl { get; set; } = new();
    public SshConfiguration Ssh { get; set; } = new();
}