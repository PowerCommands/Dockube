namespace PainKiller.DockubeClient.Configuration;
public class SshConfiguration
{
    public string Name { get; set; } = string.Empty;
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 22;
    public string UserName { get; set; } = string.Empty;
}