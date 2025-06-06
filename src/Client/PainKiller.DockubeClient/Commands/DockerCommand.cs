namespace PainKiller.DockubeClient.Commands;

public class DockerCommand(string identifier) : ProxyCommando<CommandPromptConfiguration>(identifier)
{
    public override void OnInitialized()
    {
        base.OnInitialized();
        if(Configuration.Dockube.AutostartDockerDesktop) DockerService.Default.EnsureDockerRunning();
    }
}