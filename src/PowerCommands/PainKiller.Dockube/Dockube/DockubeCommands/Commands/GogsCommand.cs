using RestSharp;
using System.Text;
using DockubeCommands.DomainObjects;
using PainKiller.PowerCommands.Security.Services;

namespace DockubeCommands.Commands;

[PowerCommandTest(         tests: " ")]
[PowerCommandDesign( description: "Connect to your gogs repo",
                         example: "gogs")]
public class GogsCommand : CommandBase<PowerCommandsConfiguration>
{
    public GogsCommand(string identifier, PowerCommandsConfiguration configuration) : base(identifier, configuration) { }

    public override RunResult Run()
    {
        Console.Write("Password: ");
        var password = PasswordPromptService.Service.ReadPassword();
        Console.WriteLine();
        var repo = GetRepo(Configuration.GogsUserName, Configuration.GogsMainRepo, password);
        WriteSuccessLine($"{repo.name} {repo.description} created: {repo.created_at} ");
        return Ok();
    }

    private GitRepo GetRepo(string userName,string repoName, string password)
    {
        var client = new RestClient(Configuration.GogsServer);
        var request = new RestRequest($"repos/{userName}/{repoName}");
        request.AddUrlSegment("username", userName);
        request.AddUrlSegment("reponame", repoName);

        request.AddHeader("Authorization", "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes($"{userName}:{password}")));
        var response = client.Get<GitRepo>(request) ?? new GitRepo { name = "-", description = "?" };
        return response;
    }
}