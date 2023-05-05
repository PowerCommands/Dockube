using RestSharp;
using System.Text;
using System.Text.Json;
using DockubeCommands.DomainObjects;

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
        var content = GetContent(repoName, path);
        if (string.IsNullOrEmpty(content.sha)) return $"File {path} not found";

        //var client = new HttpClient();
        //client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Token", _accessToken );
        //var baseUrl = $"{_gogsServer}/repos/{_userName}/{repoName}/contents/{path}?sha={content.sha}";
        //var deleteResponse = client.DeleteAsync(baseUrl).Result;


        var request = GetContentRestRequest(repoName, Method.Delete, content.path);
        request.AddParameter("path", content.path);
        request.AddParameter("sha", content.sha);
        request.AddParameter("branch","master");
        request.AddParameter("committer.name", _userName);
        request.AddParameter("committer.email","harri.klingsten@gmail.com");
        

        request.AddJsonBody(new { message = $"Delete file {content.path}", sha = content.sha });
        
        
        var response = _client.Execute(request);
        return $"File {content.path} sha: {content.sha} deleted. status code: {response.StatusCode}";
    }

    public GogsContent GetContent(string repoName, string path)
    {
        var request = GetContentRestRequest(repoName, Method.Get, path);
        request.AddUrlSegment("path", path);
        var response = _client.Execute(request);
        if(string.IsNullOrEmpty(response.Content)) return new GogsContent();
        var content = JsonSerializer.Deserialize<GogsContent>(response.Content) ?? new GogsContent();
        return content;
    }

    public string CommitChanges(string repoName, string commitMessage)
    {
        var request = GetRestRequest(repoName, Method.Post);
        request.AddJsonBody(new { message = commitMessage, content = "commit content", branch_name = "master" });
        var response = _client.Execute(request);
        return $"Commit status code: {response.StatusCode}";
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