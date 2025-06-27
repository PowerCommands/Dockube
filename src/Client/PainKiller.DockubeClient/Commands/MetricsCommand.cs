using PainKiller.CommandPrompt.CoreLib.Modules.ShellModule.Services;

namespace PainKiller.DockubeClient.Commands;

[CommandDesign(     description: "Show node metrics", 
                       examples: ["//Show metrics","metrics"])]
public class MetricsCommand(string identifier) : ConsoleCommandBase<CommandPromptConfiguration>(identifier)
{
    public override RunResult Run(ICommandLineInput input)
    {
        //Install component if not installed
        //kubectl apply -f https://github.com/kubernetes-sigs/metrics-server/releases/latest/download/components.yaml
        
        var result = ShellService.Default.StartInteractiveProcess("kubectl", $"top pods --all-namespaces");
        Writer.WriteLine(result);
        
        return Ok();
    }
}