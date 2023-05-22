using DockubeCommands.Contracts;
using DockubeCommands.Managers;
using System.Text;

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
            WriteCodeExample("User name", $"{Configuration.GitUserName}");
            WriteCodeExample("User email", $"{Configuration.Environment.GetValue(Configuration.Constants.GitEmailEnvVar)}\n");
            WriteHeadLine("Create a repo and initialize with it a gitignore, readme and a license file.");
            WriteCodeExample("Name of the repo", $"{Configuration.GitMainRepo}\n\n");
            Write("Press any key to continue...");
            Console.ReadLine();

            var findAndReplaces = new Dictionary<string, string>
            {
                { $"##{Configuration.Constants.GitEmailEnvVar}##", Configuration.Environment.GetValue(Configuration.Constants.GitEmailEnvVar) }
            };
            TemplatesManager.FindReplaceFile(findAndReplaces, Configuration.Constants.GogsTemplateFileName, Path.Combine(AppContext.BaseDirectory, Configuration.Constants.GogsManifestDirectory));

            var workingDirectory = Path.Combine(AppContext.BaseDirectory, Configuration.Constants.GogsManifestDirectory);
            IPublishManager publisher = new PublishManager(workingDirectory);
            publisher.Publish(workingDirectory, Configuration.Constants.GogsNamespace);
            WriteCodeExample("User name", $"{Configuration.GitUserName}");
            WriteCodeExample("User email", $"{Configuration.Environment.GetValue(Configuration.Constants.GitEmailEnvVar)}\n");
            WriteHeadLine("Create a repo and initialize with it a gitignore, readme and a license file.");
            WriteCodeExample("Name of the repo", $"{Configuration.GitMainRepo}\n\n");

            var response = DialogService.YesNoDialog("At this point you need to have set up the gogs account and repo according to the instructions, continue?");
            if (!response) return Ok();
            WriteLine($"Next step is to create a secret if you have not done that before.");
            var skipSecretCreation = DialogService.YesNoDialog("Skip secret creation? (already done that)");
            var accessToken = Configuration.Secret.DecryptSecret($"##{Configuration.Constants.GitAccessTokenSecret}##");
            if (!skipSecretCreation)
            {
                WriteLine("Next step is to setup an access token, so please login and navigate to my settings/applications.");
                WriteLine("Here you can create a access token, give it any name you like, remember to copy the token.");
                WriteSuccessLine("Press any key to continue when you have created your token and copied it to memory, cause you need to store this as an encrypted secret...");
                Console.ReadLine();

                var hasSecretInitialized = SecretManager.CheckEncryptConfiguration();
                if (!hasSecretInitialized)
                {
                    WriteLine("You need to initialize the secrete feature that first with this command:");
                    WriteCodeExample("secret", "--initialize\n");
                    WriteLine("Then you can re run this command again.");
                    return Ok();
                }
                accessToken = SecretManager.CreateSecret(Configuration, $"##{Configuration.Constants.GitAccessTokenSecret}##");
            }
            var gogsManager = new GogsManager(Configuration.GitServerApi, Configuration.GitUserName, Configuration.Environment.GetValue("gitEmail"), accessToken, "master", Configuration.Constants.RepositoryPath);
            var createRepoResponse = gogsManager.CreateRepo(Configuration.GitMainRepo);
            WriteSuccessLine(createRepoResponse);
            gogsManager.InitializeRepo(Configuration.GitMainRepo);
            gogsManager.AddFilesToRepo(Configuration.GitMainRepo, Configuration.Constants.MsSqlManifestDirectory);
            gogsManager.CommitChanges(Configuration.GitMainRepo, "Main repo initialized by PowerCommands.");

            Write("Gogs setup is now complete!!\nPress any key to continue...");
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
            var token = publisher.Publish(workingDirectory, Configuration.Constants.ArgoCdNamespace);

            WriteHeadLine("You can log in to the ArgoCD admin if you want");
            WriteCodeExample("Username", "admin\n");
            WriteCodeExample("Password", $"{Encoding.UTF8.GetString(Convert.FromBase64String(token))}\n\n");
        }
        return Ok();
    }
}