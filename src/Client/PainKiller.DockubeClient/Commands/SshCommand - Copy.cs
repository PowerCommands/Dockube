namespace PainKiller.DockubeClient.Commands;

[CommandDesign(     description:  "Set active settings on different stuff like namespace, context and so on, that could be omitted when running diagnostic command.",
                    suggestions: ["host", "port", "userName"],
                       examples: ["//Set kubernetes namespace","//set namespace \"observation\""])]
public class SetCommand(string identifier) : ConsoleCommandBase<CommandPromptConfiguration>(identifier)
{
    public override RunResult Run(ICommandLineInput input)
    {
        input.TryGetOption(out var userName, Configuration.Dockube.Ssh.UserName);
        input.TryGetOption(out var port, Configuration.Dockube.Ssh.Port);
        input.TryGetOption(out var host, Configuration.Dockube.Ssh.Host);
        return Ok();
    }
}