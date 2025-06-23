namespace PainKiller.DockubeClient.Configuration;
public class DockubeConfiguration
{
    public string ManifestBasePath { get; set; } = "Manifests";
    public string DefaultDomain { get; set; } = "dockube.lan";
    public string[] Releases { get; set; } = [];
    public SslConfiguration Ssl { get; set; } = new();
    public SshConfiguration[] Ssh { get; set; } = [];
}