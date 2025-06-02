using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using IdentityModel.Client;
using LibGit2Sharp;

namespace DockubeCommands.Managers;

public class GitManager
{
    private readonly string _gogsServer;
    private readonly string _userName;
    private readonly string _userEmail;
    private readonly string _accessToken;

    public GitManager(string gogsServer, string userName, string userEmail, string accessToken)
    {
        _gogsServer = gogsServer;
        _userName = userName;
        _userEmail = userEmail;
        _accessToken = accessToken;
    }

    public void InitialiseRepository(string repoName)
    {
        var clonePath = Path.Combine(AppContext.BaseDirectory, $"repos\\{repoName}");
        var filePath = "BABAR.md";
        if (!Directory.Exists(clonePath)) Directory.CreateDirectory(clonePath);

        var repositoryUrl = $"{_gogsServer}/{_userName}/{repoName}.git";
        var options = new CloneOptions
        {
            CredentialsProvider = (_url, _user, _cred) => new UsernamePasswordCredentials
            {
                Username = _accessToken,
                Password = string.Empty
            }
        };
        Repository.Clone(repositoryUrl, clonePath, options);

        using var repo = new Repository(clonePath);
        var fileContent = "Read me...";

        // Create a new file
        var fullPath = Path.Combine(clonePath, filePath);
        File.WriteAllText(fullPath, fileContent);

        // Stage the file
        LibGit2Sharp.Commands.Stage(repo, fullPath);

        // Commit the changes
        var author = new Signature(_userName, _userEmail, DateTimeOffset.Now);
        var committer = author;
        var commit = repo.Commit("Added a new file", author, committer);

        Console.WriteLine("Commit created: " + commit.Sha);

        // Push the changes
        var remote = repo.Network.Remotes["origin"];
        var pushOptions = new PushOptions
        {
            CredentialsProvider = (_url, _user, _cred) => new UsernamePasswordCredentials
            {
                Username = _accessToken,
                Password = string.Empty
            }
        };
        repo.Network.Push(remote, $"refs/heads/master:{commit.Id.Sha}", pushOptions);
    }

    public void CreateRepository(string repoName)
    {
        using (var repo = new Repository())
        {
            var repoPath = Repository.Init(repoName);
            var signature = new Signature(_userName, _userEmail, DateTimeOffset.Now);
            var newRepo = new Repository(repoPath);

            // Set the remote origin URL
            var remote = newRepo.Network.Remotes.Add("origin", _gogsServer);

            // Create an empty initial commit
            var commit = newRepo.Commit("Initial commit", signature, signature);

            var pushOptions = new PushOptions
            {
                CredentialsProvider = (_url, _user, _cred) => new UsernamePasswordCredentials
                {
                    Username = _accessToken,
                    Password = string.Empty
                }
            };
            // Push the commit to the remote origin
            newRepo.Network.Push(remote, $"refs/heads/master:{commit.Id.Sha}", pushOptions);
        }

        Console.WriteLine("New repository created successfully on Gogs.");
    }

    public void DeleteRepository(string repoName)
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", $"token {_accessToken}");

        var url = $"{_gogsServer}/api/v1/repos/{_userName}/{repoName}";
        var response = client.DeleteAsync(url).Result;

        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine("Repository deleted successfully from Gogs.");
        }
        else
        {
            Console.WriteLine($"Error deleting repository: {response.ReasonPhrase}");
        }
    }

    public async Task<string> CreateRepositoryAsync(string repoName)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", $"token {_accessToken}");

        client.BaseAddress = new Uri(_gogsServer);
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var parameters = new
        {
            name = repoName,
            description = "My new repository"
        };
        var json = JsonSerializer.Serialize(parameters);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync($"/api/v1/admin/users/{_userName}/repos", content);

        return response.IsSuccessStatusCode ? "New repository created successfully on Gogs." : $"Error creating repository: {response.ReasonPhrase}";
    }

    public void PushToRepository(string repositoryUrl, string repositoryPath)
    {
        // Configure git
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = "config --global user.name \"Your Name\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            }
        };
        process.Start();
        process.WaitForExit();

        process.StartInfo.Arguments = "config --global user.email \"you@example.com\"";
        process.Start();
        process.WaitForExit();

        process.StartInfo.Arguments = $"config --global http.extraHeader \"Authorization: basic {_accessToken}\"";
        process.Start();
        process.WaitForExit();

        // Clone the repository
        process.StartInfo.Arguments = $"clone {repositoryUrl}";
        process.Start();
        process.WaitForExit();

        // Create a file in the repository
        File.WriteAllText(Path.Combine(repositoryPath, "README.md"), "# My Repository");

        // Add the file to the repository
        process.StartInfo.Arguments = "add .";
        process.StartInfo.WorkingDirectory = repositoryPath;
        process.Start();
        process.WaitForExit();

        // Commit the changes
        process.StartInfo.Arguments = "commit -m \"Initial commit\"";
        process.Start();
        process.WaitForExit();

        // Push the changes
        process.StartInfo.Arguments = "push";
        process.Start();
        process.WaitForExit();

        Console.WriteLine("Commit pushed successfully to repository.");
    }

}