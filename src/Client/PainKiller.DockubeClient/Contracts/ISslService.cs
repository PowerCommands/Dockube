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
    string ExportToPfx(string commonName, string intermediateDirectory, string outputFolder, string password = "");
    CertificateInfo InspectCertificate(string certPath);
    bool CertificateExists(string commonName, string outputFolder);
    bool PemFileExists(string commonName, string outputFolder);
}