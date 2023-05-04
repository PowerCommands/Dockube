using DockubeCommands.Contracts;
using DockubeCommands.Managers;

namespace DockubeCommands.Commands;

[PowerCommandDesign( description: "Startup your Dockube environment",
                         options: "confirm",
                         example: "startup")]
public class StartupCommand : CommandBase<PowerCommandsConfiguration>
{
    public StartupCommand(string identifier, PowerCommandsConfiguration configuration) : base(identifier, configuration) { }

    public override RunResult Run()
    {
        var confirmed = !HasOption("confirm") || DialogService.YesNoDialog("Do you want to start Docker desktop?");
        if (confirmed)
        {
            WriteSuccessLine("Docker Desktop starting please wait...");
            var fullFileName = Path.Combine(Configuration.PathToDockerDesktop, "Docker Desktop.exe");
            ShellService.Service.Execute(fullFileName, arguments: "", workingDirectory: "", WriteLine, fileExtension: "");
            Thread.Sleep(15000);
            WriteSuccessLine("Docker Desktop started");
        }
        confirmed = !HasOption("confirm") || DialogService.YesNoDialog("Do you want to deploy ArgoCD?");
        if (confirmed)
        {
            var workingDirectory = Path.Combine(AppContext.BaseDirectory, "Manifests", "argocd");
            IPublishManager publisher = new PublishManager(workingDirectory);
            publisher.Publish(workingDirectory, "argocd");
        }
        confirmed = !HasOption("confirm") || DialogService.YesNoDialog("Do you want to deploy Git server?");
        if (confirmed)
        {
            var workingDirectory = Path.Combine(AppContext.BaseDirectory, "Manifests", "gogs");
            IPublishManager publisher = new PublishManager(workingDirectory);
            publisher.Publish(workingDirectory, "gogs");
        }
        return Ok();
    }
}