using PainKiller.CommandPrompt.CoreLib.Modules.ShellModule.Services;

namespace PainKiller.DockubeClient.Commands;

[CommandDesign(     description: "Dockube -  See information about used certificates in the Dockube platform.", 
                      arguments: ["<Mode>"],
                        options: [""],
                    suggestions: ["status"],
                       examples: ["//View status","certificates"])]
public class CertificatesCommand(string identifier) : ConsoleCommandBase<CommandPromptConfiguration>(identifier)
{
    public override RunResult Run(ICommandLineInput input)
    { ;
        ShowCertOverview();
        return Ok();
    }
    private RunResult ShowCertOverview()
    {
        Writer.WriteHeadLine("Certificate Overview");
        
        Writer.WriteLine("Local Certificates on Disk:");
        var certFolder = Path.Combine(Configuration.Dockube.Ssl.Output, "certificate");
        if (Directory.Exists(certFolder))
        {
            var sslService = SslService.Default;
            foreach (var file in Directory.GetFiles(certFolder, "*.crt"))
            {
                try
                {
                    var info = sslService.InspectCertificate(file);
                    Writer.WriteLine($"- {Path.GetFileName(file)} CN: {info.SubjectCn} Issuer: {info.Issuer} Valid to: {info.ValidTo}");
                }
                catch (Exception ex)
                {
                    Writer.WriteLine($"- {Path.GetFileName(file)}: Failed to inspect certificate ({ex.Message})");
                }
            }
        }
        else
        {
            Writer.WriteLine($"Folder not found: {certFolder}");
        }

        Writer.WriteLine("TLS Secrets in Cluster:");
        ShellService.Default.RunTerminalUntilUserQuits("kubectl",@"get secrets --all-namespaces -o jsonpath=""{range .items[?(@.type=='kubernetes.io/tls')]}{.metadata.namespace}/{.metadata.name}{'\n'}{end}""");
        
        return Ok();
    }

}