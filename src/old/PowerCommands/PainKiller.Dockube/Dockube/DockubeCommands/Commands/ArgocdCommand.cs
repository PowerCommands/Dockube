using System.Text;
using DockubeCommands.DomainObjects;

namespace DockubeCommands.Commands;

[PowerCommandDesign( description: "Port forward ArgoCD and startup the ArgoCD admin UI",
                         example: "argocd")]
public class ArgocdCommand : CommandBase<PowerCommandsConfiguration>
{
    public ArgocdCommand(string identifier, PowerCommandsConfiguration configuration) : base(identifier, configuration) { }

    public override RunResult Run()
    {
        var workingDirectory = Path.Combine(AppContext.BaseDirectory, Configuration.Constants.ArgoCdManifestDirectory);
        var processMetadata = StorageService<ProcessMetadata>.Service.GetObject(Path.Combine(workingDirectory, "argocd-04-decode-secret.json"));


        ShellService.Service.Execute(processMetadata.Name, processMetadata.Args, "", ReadLine, "", waitForExit: processMetadata.WaitForExit, useShellExecute: processMetadata.UseShellExecute, disableOutputLogging: processMetadata.DisableOutputLogging);
        var password = processMetadata.Base64Decode ?  Encoding.UTF8.GetString(Convert.FromBase64String(LastReadLine)) : LastReadLine;
        ShellService.Service.Execute("kubectl","port-forward svc/argocd-server -n argocd 8080:443", workingDirectory:"", writeFunction: WriteLine, useShellExecute: true, disableOutputLogging: false);
        ShellService.Service.OpenWithDefaultProgram(Configuration.ArgoCdAdmin);

        Console.WriteLine("Username: admin");
        Console.WriteLine($"Password: {password}");

        return Ok();
    }
}