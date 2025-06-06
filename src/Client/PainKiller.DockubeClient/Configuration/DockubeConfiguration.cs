namespace PainKiller.DockubeClient.Configuration;
public class DockubeConfiguration
{
    public bool AutostartDockerDesktop { get; set; }
    public SslConfiguration Ssl { get; set; } = new();
}