namespace PainKiller.DockubeClient.Configuration;

public class SslConfiguration
{
    public string ExecutablePath { get; set; } = string.Empty;
    public string Output { get; set; } = string.Empty;
    public string DefaultName { get; set; } = "Dockube";
    public string DefaultCa { get; set; } = "DockubeCA";
}