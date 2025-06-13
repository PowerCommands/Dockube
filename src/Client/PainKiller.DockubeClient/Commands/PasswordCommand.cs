using PainKiller.CommandPrompt.CoreLib.Modules.ShellModule.Services;
using PainKiller.DockubeClient.Extensions;
using System.Text;

namespace PainKiller.DockubeClient.Commands;

[CommandDesign(     description: "View password for a resource", 
                    suggestions: ["gitlab", "argocd"],
                       examples: ["//View default password for gitlab (if not changed later)","password gitlab"])]
public class PasswordCommand(string identifier) : ConsoleCommandBase<CommandPromptConfiguration>(identifier)
{
    public override RunResult Run(ICommandLineInput input)
    {
        var releaseName = input.Arguments.FirstOrDefault();
        if (string.IsNullOrEmpty(releaseName)) return Ok();
        var release = Configuration.Dockube.GetRelease(releaseName) ?? new DockubeRelease();
        var resource = release.Resources.FirstOrDefault(r => r.SecretDescriptors.Length > 0);
        if (resource == null) return Ok();
        var base64Encoded = ShellService.Default.StartInteractiveProcess("kubectl.exe", $"get secret {resource.SecretDescriptors.First().Key} -n {release.Namespace} -o jsonpath='{{.data.password}}'").Trim().Replace("'","");
        var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(base64Encoded));
        Console.WriteLine(decoded);
        return Ok();
    }
}