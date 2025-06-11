namespace PainKiller.DockubeClient.DomainObjects;

public class CertificateRequest
{
    public string SubjectCn { get; set; } = string.Empty;
    public string KeyUsage { get; set; } = "serverAuth";
    public int ValidDays { get; set; } = 3650;
}