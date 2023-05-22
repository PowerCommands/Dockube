using DockubeCommands.Managers;

namespace DockubeCommands.Commands;

[PowerCommandTest(         tests: " ")]
[PowerCommandDesign( description: "Connect to your gogs repo",
                         options: "create|repo|path|content",
                         example: "gogs")]
public class GogsCommand : CommandBase<PowerCommandsConfiguration>
{
    public GogsCommand(string identifier, PowerCommandsConfiguration configuration) : base(identifier, configuration) { }

    public override RunResult Run()
    {
        var create = GetOptionValue("create");
        var path = GetOptionValue("path");
        var repoName = GetOptionValue("repo");
        var content = GetOptionValue("content");
        var commit = false;

        var accessToken = Configuration.Secret.DecryptSecret("##gitAT##");
        var gogsManager = new GogsManager(Configuration.GitServerApi, Configuration.GitUserName, Configuration.Environment.GetValue("gitEmail"), accessToken, "master", Configuration.Constants.RepositoryPath);
        var gitManager = new GitManager(Configuration.GitServer, Configuration.GitUserName, Configuration.Environment.GetValue("gitEmail"), accessToken);

        if (!string.IsNullOrEmpty(create))
        {
            CreateRepo(gogsManager, gitManager, create);
            return Ok();
        }

        var repo = gogsManager.GetRepo(repoName);
        WriteSuccessLine($"{repo.name} {repo.description} created: {repo.created_at} ");

        var tree = gogsManager.GeTreeResponse(repoName);
        foreach (var treeItem in tree.Tree) WriteLine($"{treeItem.Path} {treeItem.Type}");

        if (!string.IsNullOrEmpty(content) && !string.IsNullOrEmpty(path))
        {
            var response = gogsManager.AddFileToRepo(repoName, path, content);
            commit = true;
            WriteSuccessLine($"{response}");
        }
        else if (!string.IsNullOrEmpty(path))
        {
            var response = gogsManager.DeleteFileFromRepo(repoName, path);
            commit = true;
            WriteSuccessLine($"{response}");
        }
        if (commit)
        {
            var response = gogsManager.CommitChanges(repoName, "Commit made with Dockube PowerCommands");
            WriteSuccessLine($"{response}");
        }
        return Ok();
    }

    public void CreateRepo(GogsManager gogsManager, GitManager gitManager, string repoName)
    {
        gitManager.CreateRepository(repoName);
        //var response = gogsManager.CreateRepo(repoName);
        //WriteSuccessLine(response);
        //PauseService.Pause(3);
        //gitManager.InitialiseRepository(repoName);

        //gogsManager.InitializeRepo(repoName);
        //gogsManager.CommitChanges(repoName, "Initialization of the repo by PowerCommands.");
        //gogsManager.AddFilesToRepo(repoName, Path.Combine(AppContext.BaseDirectory, $"{Configuration.Constants.MsSqlManifestDirectory}"));
        //gogsManager.CommitChanges(repoName, "Initialization of the repo by PowerCommands.");
    }
}