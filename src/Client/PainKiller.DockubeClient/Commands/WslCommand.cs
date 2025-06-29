namespace PainKiller.DockubeClient.Commands;

[CommandDesign(description: "Dockube -  Start a WSL terminal session.", 
                  examples: ["//Start wsl terminal session","wsl"])]
public class WslCommand(string identifier) : TerminalCommando<CommandPromptConfiguration>(identifier)
{
    public override RunResult Run(ICommandLineInput input)
    {
        
        var kConfig = $"export export KUBECONFIG=/mnt/c/Users/{Environment.UserName}/.kube/config-k3s.yaml";
        TextCopy.ClipboardService.SetText(kConfig);
        Writer.WriteLine($"Command below copied to clipboard, run that to change kubernetes environment temporarily on you WSL instance.\n{kConfig}");
        
        return RunTerminal(input);
    }
}