using Org.BouncyCastle.Tls;
using PainKiller.CommandPrompt.CoreLib.Core.Presentation;
using PainKiller.CommandPrompt.CoreLib.Modules.InfoPanelModule.Services;
using PainKiller.CommandPrompt.CoreLib.Modules.ShellModule.Services;
using PainKiller.DockubeClient.Extensions;
using PainKiller.DockubeClient.Managers;
using PainKiller.ReadLine.Managers;

namespace PainKiller.DockubeClient.Commands;

[CommandDesign(     description: "Dockube -  Upload a file to a pod", 
                      arguments: ["<namespace>"],
                        options: ["ssh"],
                       examples: ["//Connect to a pod in gitlab namespace, chose a file to upload","upload gitlab"])]
public class UploadCommand(string identifier) : ConsoleCommandBase<CommandPromptConfiguration>(identifier)
{
    private readonly string _identifier = identifier;
    public override void OnInitialized()
    {
        var items = KubeEnvironmentManager.GetNamespaces().ToList();
        items.InsertRange(0, Configuration.Dockube.Ssh.Select(s => s.Name));
        SuggestionProviderManager.AppendContextBoundSuggestions(_identifier, items.ToArray());
        base.OnInitialized();
    }
    public override RunResult Run(ICommandLineInput input)
    {
        var ns = input.Arguments.First();
        if (string.IsNullOrWhiteSpace(ns)) return Nok("Namespace are required.");
        if (input.HasOption("ssh")) return UploadToSsh(ns);
        
        var pods = ShellService.Default.GetNames("kubectl", $"get pods -n {ns}");
        var podIdentity = ListService.ListDialog("Select your pod", pods, autoSelectIfOnlyOneItem: false).First().Value;
        Console.WriteLine("");

        var dir = new DirectoryInfo(Environment.CurrentDirectory);
        var file = ListService.ListDialog("Select file to copy", dir.GetFiles().Select(f => f.Name).ToList(), autoSelectIfOnlyOneItem: false).First().Value;

        var args = $"-n {ns} exec -i {podIdentity} -- tee /tmp/{file} < ./{file}";
        ShellService.Default.RunCommandWithFileInput("kubectl", args, file);

        Writer.WriteHeadLine($"File uploaded to /tmp/{file} on pod {podIdentity}");
        
        InfoPanelService.Instance.Update();
        return Ok($"File uploaded to /tmp/{file} on pod {podIdentity}");
    }

    public RunResult UploadToSsh(string name)
    {
        var dir = new DirectoryInfo(Environment.CurrentDirectory);
        var file = ListService.ListDialog("Select file to copy", dir.GetFiles().Select(f => f.Name).ToList(), autoSelectIfOnlyOneItem: false).First().Value;
        var config = Configuration.Dockube.Ssh.First(s => s.Name == name);

        var args = $"./{file} {config.UserName}@{config.Host}:~";

        try
        {
            ShellService.Default.RunCommandWithFileInput("scp", args, file);
        }
        catch
        {
            Writer.ClearRow(Math.Abs(Console.CursorTop-1));
        }

        Writer.WriteHeadLine($"File uploaded to /home/{file} on {name}");

        InfoPanelService.Instance.Update();
        return Ok($"File uploaded to /home/{file} on {name}");
    }
}