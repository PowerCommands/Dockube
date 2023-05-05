using RestSharp;
using System.Text;
using DockubeCommands.DomainObjects;
using System.IO;

namespace DockubeCommands.Managers;

public class GogsManager
{
    private readonly string _userName;
    private readonly string _accessToken;
    private readonly RestClient _client;

    public GogsManager(string gogsServer, string userName, string accessToken)
    {
        _userName = userName;
        _accessToken = accessToken;
        _client = new RestClient(gogsServer);
    }

    public GitRepo GetRepo(string repoName)
    {
        var request = GetRestRequest(repoName, Method.Get);
        var response = _client.Get<GitRepo>(request) ?? new GitRepo { name = "-", description = "?" };
        return response;
    }

    public string AddFileToRepo(string repoName, string path, string fileContent)
    {
        var request = GetContentRestRequest(repoName, Method.Put, path);
        request.AddUrlSegment("path", path);
        request.AddParameter("application/json", "{\"message\":\"Add new file\",\"content\":\"" + Convert.ToBase64String(Encoding.UTF8.GetBytes(fileContent)) + "\"}", ParameterType.RequestBody);
        var response = _client.Execute(request);
        return $"{response.Content}";
    }

    public string DeleteFileFromRepo(string repoName, string path)
    {
        var request = GetRestRequest(repoName, Method.Delete);
        request.AddUrlSegment("path", path);
        var response = _client.Execute(request);
        return $"{response.Content}";
    }

    public string CommitChanges(string repoName)
    {
        var request = GetRestRequest(repoName, Method.Post);
        request.AddJsonBody(new { message = "Commit message", content = "commit content", branch_name = "master" });
        var response = _client.Execute(request);
        return $"{response.Content}";
    }

    private RestRequest GetRestRequest(string repoName, Method method)
    {
        var request = new RestRequest($"repos/{_userName}/{repoName}", method);
        return request;
    }

    private RestRequest GetContentRestRequest(string repoName, Method method, string path)
    {
        var request = new RestRequest($"repos/{_userName}/{repoName}/contents/{path}", method);
        request.AddUrlSegment("username", _userName);
        request.AddUrlSegment("reponame", repoName);
        request.AddHeader("Authorization", $"token {_accessToken}");
        return request;
    }
}