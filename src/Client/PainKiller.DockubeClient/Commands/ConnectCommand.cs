using PainKiller.CommandPrompt.CoreLib.Core.Presentation;
using PainKiller.CommandPrompt.CoreLib.Modules.InfoPanelModule.Services;
using PainKiller.CommandPrompt.CoreLib.Modules.ShellModule.Services;
using PainKiller.DockubeClient.Extensions;
using PainKiller.ReadLine.Managers;

namespace PainKiller.DockubeClient.Commands;

[CommandDesign(     description: "Dockube -  Connect to pod in provided namespace running in cluster", 
                      arguments: ["<namespace>","<instance-name>"],
                       examples: ["//Connect to a pod in gitlab namespace","connect gitlab"])]
public class ConnectCommand(string identifier) : ConsoleCommandBase<CommandPromptConfiguration>(identifier)
{
    private readonly string _identifier = identifier;
    public override void OnInitialized()
    {
        var namespaces = ShellService.Default.GetNames("kubectl", $"get namespaces");
        namespaces.Insert(0, "docker-desktop (connect to Docker container)");
        SuggestionProviderManager.AppendContextBoundSuggestions(_identifier, namespaces.ToArray());
        base.OnInitialized();
    }
    public override RunResult Run(ICommandLineInput input)
    {
        var ns = input.Arguments.First();
        if (ns.StartsWith("docker-desktop"))
        {
            var containers = ShellService.Default.GetNames("docker", "ps --format \"{{.Names}}\"");
            var containerName = ListService.ListDialog("Select your container", containers).First().Value;
            Console.WriteLine("");
            ShellService.Default.RunTerminalUntilUserQuits("docker", $"exec -it {containerName} sh");
            return Ok("docker-desktop run.");
        }
        if (string.IsNullOrWhiteSpace(ns)) return Nok("Namespace are required.");
        
        var pods = ShellService.Default.GetNames("kubectl", $"get pods -n {ns}");
        var podIdentity = ListService.ListDialog("Select your pod", pods, autoSelectIfOnlyOneItem: false).First().Value;
        Console.WriteLine("");
        ShellService.Default.RunTerminalUntilUserQuits("kubectl", $"exec -it {podIdentity} -n {ns} -- sh");

        InfoPanelService.Instance.Update();
        return Ok("kubectl runned.");
    }
}