using PainKiller.CommandPrompt.CoreLib.Modules.ShellModule.Services;
using PainKiller.DockubeClient.Contracts;
namespace PainKiller.DockubeClient.Services;
public class SslService(string executablePath) : ISslService
{
    private readonly string _fullPath = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), executablePath), "openssl.exe");
    public string GetVersion()
    {
        if (string.IsNullOrEmpty(executablePath)) return "";
        var version = ShellService.Default.StartInteractiveProcess(_fullPath, "version");
        return version;
    }

    public string CreateRootCertificate(string name, int validDays, string outputFolder)
    {
        if(Directory.Exists(outputFolder) == false) Directory.CreateDirectory(outputFolder);
        var keyPath = Path.Combine(outputFolder, "root.key");
        var crtPath = Path.Combine(outputFolder, "root.crt");

        var arguments = $"req -x509 -new -nodes -keyout \"{keyPath}\" -out \"{crtPath}\" -subj \"/CN={name}\" -days {validDays} -newkey rsa:4096";
        var result = ShellService.Default.StartInteractiveProcess(_fullPath, arguments);
        return result;
    }
}