using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Dockube.Api.Controllers;


[ApiController]
[Route("[controller]")]
public class GitController : ControllerBase
{
    [HttpGet]
    public IActionResult Get(string repoName)
    {
        CreateAndPushRepository(repoName, "http://localhost:30080");
        return Ok($"Repo: {repoName} created");
    }

    public void CreateAndPushRepository(string repoName, string gogsServer)
    {
        // Initialize a new repository
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = $"init {repoName}",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            }
        };
        process.Start();
        process.WaitForExit();

        // Change directory to the new repository
        Directory.SetCurrentDirectory(repoName);

        var filePath = Path.Combine(repoName, "README.md");
        
        System.IO.File.WriteAllText(filePath, "This is a new repository.");
        process.StartInfo.Arguments = "add README.md";
        process.Start();
        process.WaitForExit();

        // Commit the changes
        process.StartInfo.Arguments = "commit -m \"Initial commit\"";
        process.Start();
        process.WaitForExit();

        // Set the remote origin URL
        process.StartInfo.Arguments = $"remote add origin {gogsServer}/{repoName}.git";
        process.Start();
        process.WaitForExit();

        // Push the commit to the remote origin
        process.StartInfo.Arguments = "push -u origin master";
        process.Start();
        process.WaitForExit();
    }

}