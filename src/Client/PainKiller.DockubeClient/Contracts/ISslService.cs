namespace PainKiller.DockubeClient.Contracts;
public interface ISslService
{
    string GetVersion();
    string CreateRootCertificate(string name, int validDays, string outputFolder);
}