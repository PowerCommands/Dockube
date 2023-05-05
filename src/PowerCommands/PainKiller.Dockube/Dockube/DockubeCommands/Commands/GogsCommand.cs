using DockubeCommands.Managers;

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
        bool commit = false;


        var accessToken = Configuration.Secret.DecryptSecret("##gogsAT##");
        
        var gogsManager = new GogsManager(Configuration.GogsServer, Configuration.GogsUserName, accessToken);
        var repo = gogsManager.GetRepo(Configuration.GogsMainRepo);
        WriteSuccessLine($"{repo.name} {repo.description} created: {repo.created_at} ");
        if (!string.IsNullOrEmpty(content) && !string.IsNullOrEmpty(path))
        {
            var response = gogsManager.AddFileToRepo(Configuration.GogsMainRepo, path, content);
            commit = true;
            WriteSuccessLine($"{response}");
        }
        else if (!string.IsNullOrEmpty(path))
        {
            var response = gogsManager.DeleteFileFromRepo(Configuration.GogsMainRepo, path);
            commit = true;
            WriteSuccessLine($"{response}");
        }
        if (commit)
        {
            var response = gogsManager.CommitChanges(Configuration.GogsMainRepo, "Commit made with Dockube PowerCommands");
            WriteSuccessLine($"{response}");
        }
        return Ok();
    }
}