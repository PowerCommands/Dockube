using DockubeCommands.Contracts;
using DockubeCommands.Managers;

namespace DockubeCommands.Commands;

[PowerCommandDesign( description: "Startup your Dockube environment",
                         options: "confirm",
                        useAsync: false,
                         example: "startup")]
public class StartupCommand : CommandBase<PowerCommandsConfiguration>
{
    public StartupCommand(string identifier, PowerCommandsConfiguration configuration) : base(identifier, configuration) { }

    public override RunResult Run()
    {
        if (!KubernetesManager.IsKubernetesAvailable().Result)
        {
            WriteCodeExample("Kubernetes environment not available","Run command dd to start up the Docker Desktop service and try again...");
            return Ok();
        }

        var confirmed = !HasOption("confirm") || DialogService.YesNoDialog("Do you want to deploy Git server?");
        if (confirmed)
        {
            WriteHeadLine("Important when gogs is starting");
            WriteCodeExample("Database type","SQLLite3\n");
            WriteCodeExample("Local address","http://localhost:30080/\n");
            WriteCodeExample("Admin account","dockube\n\n");
            var workingDirectory = Path.Combine(AppContext.BaseDirectory, "Manifests", "gogs");
            IPublishManager publisher = new PublishManager(workingDirectory);
            publisher.Publish(workingDirectory, "gogs");
            DialogService.QuestionAnswerDialog("Setup the main repo named dockube-main in Gogs?");
            DialogService.QuestionAnswerDialog("Now add a file with the subfolder path manifests so that ArgoCD can target that directory in repo in the next step?");
        }

        confirmed = !HasOption("confirm") || DialogService.YesNoDialog("Do you want to deploy ArgoCD?");
        if (confirmed)
        {
            var gitServerIp = KubernetesManager.GetIpAddress("gogs-", "gogs").Result;
            var findAndReplaces = new Dictionary<string, string>
            {
                { "##repository_url##", $"http://{gitServerIp}:3000/dockube/dockube-main" },
                { "##repository_path##", $"manifests" }
            };
            var templateFileName = "argocd-05-add-application-dockube.yaml";
            TemplatesManager.FindReplaceFile(findAndReplaces, Path.Combine(AppContext.BaseDirectory, $"Manifests\\templates\\{templateFileName}"),Path.Combine(AppContext.BaseDirectory, $"Manifests\\argocd\\{templateFileName}"));
            var workingDirectory = Path.Combine(AppContext.BaseDirectory, "Manifests", "argocd");
            IPublishManager publisher = new PublishManager(workingDirectory);
            publisher.Publish(workingDirectory, "argocd");
        }
        return Ok();
    }
}