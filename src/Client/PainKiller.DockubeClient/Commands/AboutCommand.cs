using PainKiller.CommandPrompt.CoreLib.Core.Presentation;
using PainKiller.CommandPrompt.CoreLib.Modules.ShellModule.Services;
using PainKiller.DockubeClient.Extensions;
using PainKiller.DockubeClient.Managers;
using PainKiller.ReadLine.Managers;

namespace PainKiller.DockubeClient.Commands;

[CommandDesign(description: "Dockube -  See detailed information about a resource (default is pod)",
                 arguments: ["<namespace>"],
                   options: ["pod", "svc", "ingress"],
                  examples: ["//Describe a pod in namespace gitlab", "about gitlab",
                             "//Describe a service in namespace gitlab", "about gitlab --svc",
                             "//Describe an ingress in namespace gitlab", "about gitlab --ingress"])]
public class AboutCommand(string identifier) : ConsoleCommandBase<CommandPromptConfiguration>(identifier)
{
    private readonly string _identifier = identifier;
    public override void OnInitialized()
    {
        var namespaces = KubeEnvironmentManager.GetNamespaces();
        SuggestionProviderManager.AppendContextBoundSuggestions(_identifier, namespaces.ToArray());
        base.OnInitialized();
    }

    public override RunResult Run(ICommandLineInput input)
    {
        var ns = input.Arguments.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(ns))
            return Nok("Namespace is required.");

        string kind;
        if (input.HasOption("svc"))
            kind = "svc";
        else if (input.HasOption("ingress"))
            kind = "ingress";
        else
            kind = "pod";

        var items = ShellService.Default.GetNames("kubectl", $"get {kind} -n {ns}");
        var name = ListService.ListDialog($"Select a {kind} to describe", items, autoSelectIfOnlyOneItem: false).First().Value;

        var result = ShellService.Default.StartInteractiveProcess("kubectl", $"describe {kind} {name} -n {ns}");
        Console.WriteLine(result);

        return Ok();
    }
}
