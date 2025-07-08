using PainKiller.CommandPrompt.CoreLib.Core.Presentation;
using PainKiller.CommandPrompt.CoreLib.Modules.InfoPanelModule.Services;
using PainKiller.CommandPrompt.CoreLib.Modules.ShellModule.Services;
using PainKiller.DockubeClient.Extensions;
using PainKiller.DockubeClient.Managers;
using PainKiller.ReadLine.Managers;

namespace PainKiller.DockubeClient.Commands;

[CommandDesign(     description: "Dockube -  Upload a file to a pod", 
                      arguments: ["<namespace>"],
                       examples: ["//Connect to a pod in gitlab namespace, chose a file to upload","upload gitlab"])]
public class UploadCommand(string identifier) : ConsoleCommandBase<CommandPromptConfiguration>(identifier)
{
    private readonly string _identifier = identifier;
    public override void OnInitialized()
    {
        var namespaces = KubeEnvironmentManager.GetNamespaces().ToList();
        SuggestionProviderManager.AppendContextBoundSuggestions(_identifier, namespaces.ToArray());
        base.OnInitialized();
    }
    public override RunResult Run(ICommandLineInput input)
    {
        var ns = input.Arguments.First();
        if (string.IsNullOrWhiteSpace(ns)) return Nok("Namespace are required.");
        
        var pods = ShellService.Default.GetNames("kubectl", $"get pods -n {ns}");
        var podIdentity = ListService.ListDialog("Select your pod", pods, autoSelectIfOnlyOneItem: false).First().Value;
        Console.WriteLine("");

        var dir = new DirectoryInfo(Environment.CurrentDirectory);
        var file = ListService.ListDialog("Select file to copy", dir.GetFiles().Select(f => f.Name).ToList(), autoSelectIfOnlyOneItem: false).First().Value;

        var args = $"-n {ns} exec -i {podIdentity} -- tee /tmp/{file} < ./{file}";
        ShellService.Default.Execute("kubectl", args);

        Writer.WriteHeadLine("Command below copied to clipboard (sometimes you need a real terminal)");
        Writer.WriteLine($"kubectl {args}");
        TextCopy.ClipboardService.SetText($"kubectl {args}");

        InfoPanelService.Instance.Update();
        return Ok($"kubectl {args}");
    }
}