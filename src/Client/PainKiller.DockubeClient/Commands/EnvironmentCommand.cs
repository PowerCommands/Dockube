using PainKiller.CommandPrompt.CoreLib.Modules.InfoPanelModule.Services;
using PainKiller.DockubeClient.Managers;

namespace PainKiller.DockubeClient.Commands;

[CommandDesign(     description: "Dockube -  Change or view the current kubernetes environment.", 
                    suggestions: ["k3s", "docker-desktop"],
                       examples: ["//Change enviroment to k3s","environment k3s"])]
public class EnvironmentCommand(string identifier) : ConsoleCommandBase<CommandPromptConfiguration>(identifier)
{
    public override RunResult Run(ICommandLineInput input)
    {
        var environment = this.GetSuggestion(input.Arguments.FirstOrDefault(), "");
        if (string.IsNullOrEmpty(environment)) return ViewEnvironment();
        var environmentManager = new KubeEnvironmentManager();
        environmentManager.SwitchEnvironment(environment);
        Thread.Sleep(1000);
        ViewEnvironment();
        return Ok();
    }
    
    public RunResult ViewEnvironment()
    {
        Writer.WriteHeadLine($"Current kubernetes environment: {KubeEnvironmentManager.GetTarget()}");
        InfoPanelService.Instance.Update();
        return Ok();
    }

    //helm get values gitlab -n gitlab --all > gitlab-effective-values.yaml
}