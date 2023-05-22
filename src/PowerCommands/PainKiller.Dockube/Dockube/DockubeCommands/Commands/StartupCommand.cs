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
            WriteHeadLine("Important! You have to register a new user when the gogs Admin appear in your browser");
            WriteCodeExample("User name",$"{Configuration.GitUserName}\n");
            WriteCodeExample("User email",$"{Configuration.Environment.GetValue(Configuration.Constants.GitEmailEnvVar)}\n\n");
            Write("Press any key to continue...");
            Console.ReadLine();
            
            var findAndReplaces = new Dictionary<string, string>
            {
                { "##ADMIN_EMAIL##", Configuration.Environment.GetValue(Configuration.Constants.GitEmailEnvVar) }
            };
            TemplatesManager.FindReplaceFile(findAndReplaces, Configuration.Constants.GogsTemplateFileName,Path.Combine(AppContext.BaseDirectory, Configuration.Constants.GogsManifestDirectory));

            var workingDirectory = Path.Combine(AppContext.BaseDirectory, Configuration.Constants.GogsManifestDirectory);
            IPublishManager publisher = new PublishManager(workingDirectory);
            publisher.Publish(workingDirectory, Configuration.Constants.GogsNamespace);
            WriteCodeExample("User name",$"{Configuration.GitUserName}\n");
            WriteCodeExample("User email",$"{Configuration.Environment.GetValue(Configuration.Constants.GitEmailEnvVar)}\n\n");
            var response = DialogService.YesNoDialog("At this point you need to have set up the gogs account according to the instructions, continue?");
            if (!response) return Ok();
            WriteHeadLine("Next step is to setup an access token, so please login and navigate to my settings/applications.");
            WriteHeadLine("Here you can create a access token, give it any name you like, remember to copy the token.");
            Write("Press any key to continue when you have created your token and copied to memory, cause you need to store this as an encrypted secret...");
            Console.ReadLine();
            WriteLine($"Next step is to create a secret, press any key to continue...");
            Console.ReadLine();
            var hasSecretInitialized = SecretManager.CheckEncryptConfiguration();
            if (!hasSecretInitialized)
            {
                WriteLine("You need to initialize the secrete feature that first with this command:");
                WriteCodeExample("secret","--initialize\n");
                WriteLine("Then you can re run this command again.");
                return Ok();
            }
            SecretManager.CreateSecret(Configuration, Configuration.Constants.GitAccessTokenSecret);
            Write("When you have created the secret, everything with the Gogs server is now setup!\nPress any key to continue...");
            Console.ReadLine();
        }

        confirmed = !HasOption("confirm") || DialogService.YesNoDialog("Do you want to deploy ArgoCD?");
        if (confirmed)
        {
            var gitServerIp = KubernetesManager.GetIpAddress(Configuration.Constants.GogsContainerStartsWith, Configuration.Constants.GogsNamespace).Result;
            var findAndReplaces = new Dictionary<string, string>
            {
                { Configuration.Constants.RepositoryUrlPlaceholder, $"http://{gitServerIp}:3000/{Configuration.GitUserName}/{Configuration.GitMainRepo}" },
                { Configuration.Constants.RepositoryPathPlaceholder, Configuration.Constants.RepositoryPath }
            };
            TemplatesManager.FindReplaceFile(findAndReplaces, Configuration.Constants.ArgoCdTemplateFileName,Path.Combine(AppContext.BaseDirectory, Configuration.Constants.ArgoCdManifestDirectory));
            var workingDirectory = Path.Combine(AppContext.BaseDirectory, Configuration.Constants.ArgoCdManifestDirectory);
            IPublishManager publisher = new PublishManager(workingDirectory);
            publisher.Publish(workingDirectory, Configuration.Constants.ArgoCdNamespace);
        }
        return Ok();
    }
}