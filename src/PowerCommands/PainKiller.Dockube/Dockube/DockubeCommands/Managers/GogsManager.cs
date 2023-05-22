using System.Net;
using RestSharp;
using System.Text;
using DockubeCommands.DomainObjects;
using System.Text.Json;

namespace DockubeCommands.Managers;

public class GogsManager
{
    private readonly string _gogsServer;
    private readonly string _userName;
    private readonly string _userEmail;
    private readonly string _accessToken;
    private readonly string _branchName;
    private readonly string _manifestsPath;
    private readonly RestClient _client;

    public GogsManager(string gogsServer, string userName, string userEmail, string accessToken, string branchName, string manifestsPath)
    {
        _gogsServer = gogsServer;
        _userName = userName;
        _userEmail = userEmail;
        _accessToken = accessToken;
        _branchName = branchName;
        _manifestsPath = manifestsPath;
        _client = new RestClient(gogsServer);
    }

    public GitRepo GetRepo(string repoName)
    {
        var request = GetRestRequest(repoName, Method.Get);
        var response = _client.Get<GitRepo>(request) ?? new GitRepo { name = "-", description = "?" };
        return response;
    }

    public TreeResponse GeTreeResponse(string repoName)
    {
        var apiUrl = $"repos/{_userName}/{repoName}/git/trees/master?recursive=true";
        // Create a request to get the repository tree
        var request = new RestRequest(apiUrl);
        request.AddUrlSegment("username", _userName);
        request.AddUrlSegment("reponame", repoName);
        request.AddHeader("Authorization", $"token {_accessToken}");

        var getResponse = _client.Execute<TreeResponse>(request);
        return getResponse.Data ?? new TreeResponse { Tree = new List<TreeItem>() };
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
        var request = GetContentRestRequest(repoName, Method.Delete, path);
        request.AddUrlSegment("path", path);
        request.AddParameter("sha", content.sha);
        request.AddParameter("branch", _branchName);
        request.AddParameter("committer.name", _userName);
        request.AddParameter("committer.email", _userEmail);


        request.AddJsonBody(new { message = $"Delete file {content.path}", sha = content.sha });
        var response = _client.Execute(request);
        return $"{response.StatusCode}";
    }

    public string CreateRepo(string repoName)
    {
        var request = new RestRequest("user/repos", Method.Post);
        request.AddHeader("Authorization", $"token {_accessToken}");
        var requestBody = new { name = repoName, description = "auto created repo", gitignores = "VisualStudio", license = "GNU General Public License v3.0"  };
        request.AddJsonBody(requestBody);
        var response = _client.Execute(request);
        return $"{response.StatusCode}";
    }

    public void InitializeRepo(string repoName)
    {
        var content = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Manifests\\gogs\\init-repo-files\\_gitignore"));
        AddFileToRepo(repoName, ".gitignore", content);

        content = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Manifests\\gogs\\init-repo-files\\LICENSE"));
        AddFileToRepo(repoName, "LICENSE", content);

        content = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Manifests\\gogs\\init-repo-files\\README.md"));
        AddFileToRepo(repoName, "README.md", content);
    }

    public void AddFilesToRepo(string repoName, string pathToFiles, string filter = "*.yaml")
    {
        foreach (var fileName in Directory.GetFiles(pathToFiles, filter))
        {
            var fileInfo = new FileInfo(fileName);
            var content = File.ReadAllText(fileName);
            AddFileToRepo(repoName, $"{_manifestsPath}/{fileInfo.Name}", content);
            Console.WriteLine($"Added file {fileInfo.Name} to {_manifestsPath}");
        }
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
    public string CommitChanges(string repoName, string message)
    {
        var request = GetRestRequest(repoName, Method.Post);
        request.AddJsonBody(new { message = message, content = "commit content", branch_name = _branchName });
        var response = _client.Execute(request);
        return $"Commit: {response.StatusCode}";
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