using DockubeCommands.Contracts;
using DockubeCommands.Managers;

namespace DockubeCommands.Commands;

[PowerCommandDesign( description: "Startup your Dockube environment",
                         example: "startup")]
public class StartupCommand : CommandBase<PowerCommandsConfiguration>
{
    public StartupCommand(string identifier, PowerCommandsConfiguration configuration) : base(identifier, configuration) { }

    public override RunResult Run()
    {
        var fullFileName = Path.Combine(Configuration.PathToDockerDesktop, "Docker Desktop.exe");
        ShellService.Service.Execute(fullFileName, arguments: "", workingDirectory: "", WriteLine, fileExtension: "");
        Thread.Sleep(5000);
        WriteSuccessLine("Docker Desktop started");

        var workingDirectory = Path.Combine(AppContext.BaseDirectory, "Manifests", "argocd");

        IPublishManager publisher = new PublishManager(workingDirectory);
        publisher.Publish(workingDirectory, "argocd");
        return Ok();
    }
}