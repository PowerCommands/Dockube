namespace PainKiller.DockubeClient.DomainObjects;
public class DockubeResource
{
    public string Type { get; set; } = "kubectl";
    public string Source { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public CertificateRequest[] Certificates { get; set; } = [];
    public string[] Before { get; set; } = [];
    public string[] After { get; set; } = [];
    public Dictionary<string, string> Parameters { get; set; } = new();
    public SecretDescriptor[] SecretDescriptors { get; set; } = [];
}