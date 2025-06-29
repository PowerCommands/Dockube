namespace PainKiller.DockubeClient.Contracts;

public interface ICertificateManager
{
    CreateCertificateResponse CreateCertificate(CertificateRequest request);
}