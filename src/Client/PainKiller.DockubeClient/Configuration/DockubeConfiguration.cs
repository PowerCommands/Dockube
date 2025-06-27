namespace PainKiller.DockubeClient.Configuration;
public class DockubeConfiguration
{
    public string ManifestsPath { get; set; } = "Manifests";
    public string TemplatesPath { get; set; } = "Templates";
    public string BackupPath { get; set; } = "Backups";
    public string DefaultDomain { get; set; } = "dockube.lan";
    public string[] Releases { get; set; } = [];
    public SslConfiguration Ssl { get; set; } = new();
    public SshConfiguration[] Ssh { get; set; } = [];
}