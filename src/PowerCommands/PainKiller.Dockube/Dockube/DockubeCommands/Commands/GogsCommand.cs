using DockubeCommands.Managers;
using PainKiller.PowerCommands.Security.Services;

namespace DockubeCommands.Commands;

[PowerCommandTest(         tests: " ")]
[PowerCommandDesign( description: "Connect to your gogs repo",
                         options: "path|content",
                         example: "gogs")]
public class GogsCommand : CommandBase<PowerCommandsConfiguration>
{
    public GogsCommand(string identifier, PowerCommandsConfiguration configuration) : base(identifier, configuration) { }

    public override RunResult Run()
    {
        var path = GetOptionValue("path");
        var content = GetOptionValue("content");


        var accessToken = Configuration.Secret.DecryptSecret("##gogsAT##");
        
        var gogsManager = new GogsManager(Configuration.GogsServer, Configuration.GogsUserName, accessToken);
        var repo = gogsManager.GetRepo(Configuration.GogsMainRepo);
        WriteSuccessLine($"{repo.name} {repo.description} created: {repo.created_at} ");

        if (!string.IsNullOrEmpty(content) && !string.IsNullOrEmpty(path))
        {
            var response = gogsManager.AddFileToRepo(Configuration.GogsMainRepo, path, content);
            WriteSuccessLine($"{response}");
        }
        else if (!string.IsNullOrEmpty(path))
        {
            var response = gogsManager.DeleteFileFromRepo(Configuration.GogsMainRepo, path);
            WriteSuccessLine($"{response}");
        }

        if (!string.IsNullOrEmpty(content))
        {
            var response = gogsManager.CommitChanges(Configuration.GogsMainRepo);
            WriteSuccessLine($"{response}");
        }
        return Ok();
    }
}