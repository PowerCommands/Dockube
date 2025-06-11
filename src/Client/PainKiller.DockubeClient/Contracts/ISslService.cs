using PainKiller.DockubeClient.DomainObjects;

namespace PainKiller.DockubeClient.Contracts;
public interface ISslService
{
    string GetVersion();
    string CreateRootCertificate(string name, int validDays, string outputFolder);
    string CreateIntermediateCertificate(string name, int validDays, string outputFolder);
    string CreateRequestForTls(string commonName, string outputFolder, IEnumerable<string>? sanList = null);
    string CreateRequestForAuth(string commonName, string outputFolder, IEnumerable<string>? sanList = null);
    string CreateAndSignCertificate(string commonName, int validDays, string outputFolder, string caName, IEnumerable<string>? sanList = null);
    string ExportFullChainPemFile(string host, string intermediateDirectory, string output);
    CertificateInfo InspectCertificate(string certPath);
}