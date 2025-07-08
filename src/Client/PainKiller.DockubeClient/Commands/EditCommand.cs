using PainKiller.CommandPrompt.CoreLib.Core.Presentation;
using PainKiller.CommandPrompt.CoreLib.Modules.InfoPanelModule.Services;
using PainKiller.CommandPrompt.CoreLib.Modules.ShellModule.Services;
using PainKiller.DockubeClient.Extensions;
using PainKiller.DockubeClient.Managers;
using PainKiller.ReadLine.Managers;

namespace PainKiller.DockubeClient.Commands;

[CommandDesign(     description: "Dockube -  Edit a yaml file", 
                      arguments: ["<namespace>"],
                       examples: ["//Connect to a pod in gitlab namespace","edit gitlab"])]
public class EditCommand(string identifier) : ConsoleCommandBase<CommandPromptConfiguration>(identifier)
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
        
        string[] commonKubernetesObjects = ["pods", "deployments", "services", "ingresses", "configmaps", "secrets", "statefulsets", "daemonsets", "cronjobs", "jobs"];
        var objectType = ListService.ListDialog("Select your pod", commonKubernetesObjects.ToList(), autoSelectIfOnlyOneItem: false).First().Value;
        Console.WriteLine("");

        var resources = ShellService.Default.GetNames("kubectl", $"get {objectType} -n {ns}");
        var resourceName = ListService.ListDialog("Select your resource to edit", resources, autoSelectIfOnlyOneItem: false).First().Value;

        
        var args = $"edit {objectType} {resourceName} -n {ns}";
        Writer.WriteLine($"\nkubectl {args}");

        ShellService.Default.RunTerminalUntilUserQuits("kubectl", args);
        
        InfoPanelService.Instance.Update();
        return Ok($"kubectl {args}");
    }
}