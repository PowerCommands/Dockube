namespace PainKiller.DockubeClient.DomainObjects;
public class DockubeResource
{
    public string Type { get; set; } = "kubectl"; // default om inget anges
    public string Source { get; set; } = string.Empty; // kan vara path, chart name, eller inline
    public CertificateRequest[] Certificates { get; set; } = [];
    public string[] Before { get; set; } = [];
    public string[] After { get; set; } = [];
    public Dictionary<string, string> Parameters { get; set; } = new(); // för helm m.m.
}