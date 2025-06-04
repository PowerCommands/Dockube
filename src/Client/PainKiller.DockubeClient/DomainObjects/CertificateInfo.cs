namespace PainKiller.DockubeClient.DomainObjects;

public class CertificateInfo
{
    public string FileName { get; set; } = string.Empty;
    public string SubjectCn { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }
    public bool IsValidNow { get; set; }
    public string ThumbprintSha1 { get; set; } = string.Empty;
    public string? KeyUsage { get; set; }
    public List<string?> ExtendedKeyUsages { get; set; } = [];
    public List<string> SubjectAlternativeNames { get; set; } = [];
}
