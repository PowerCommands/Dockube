using PainKiller.CommandPrompt.CoreLib.Core.Presentation;
using PainKiller.CommandPrompt.CoreLib.Modules.InfoPanelModule.Services;
using PainKiller.CommandPrompt.CoreLib.Modules.ShellModule.Services;

namespace PainKiller.DockubeClient.Commands;

[CommandDesign(     description: "Dockube -  Connect to pod in provided namespace running in cluster", 
                      arguments: ["<namespace>","<instance-name>"],
                       examples: ["//Connect to a pod in gitlab namespace","connect gitlab"])]
public class ConnectCommand(string identifier) : ConsoleCommandBase<CommandPromptConfiguration>(identifier)
{
    public override RunResult Run(ICommandLineInput input)
    {
        var ns = input.Arguments.First();
        if (string.IsNullOrWhiteSpace(ns)) return Nok("Namespace are required.");
        var rows = ShellService.Default.StartInteractiveProcess("kubectl", $"get pods -n {ns}").Split('\n');
        if (rows.Length == 0)
        {
            Writer.WriteLine($"No pods found in namespace {ns}.");
            return Nok();
        }
        
        var pods = rows.Select(r => $"{r.Split(' ').FirstOrDefault()}").Where(p => !string.IsNullOrEmpty(p.Trim()) && p.Trim().ToLower() != "name").ToList();
        var pod = ListService.ListDialog("Select your pod", pods);
        var podIdentity = pod.First().Value;

        ShellService.Default.RunTerminalUntilUserQuits("kubectl", $"exec -it {podIdentity} -n {ns} -- sh");

        InfoPanelService.Instance.Update();
        return Ok();
    }
}