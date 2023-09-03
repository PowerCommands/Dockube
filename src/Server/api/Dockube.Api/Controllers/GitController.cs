using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Dockube.Api.Controllers;


[ApiController]
[Route("api/[controller]")]
public class GitController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        try
        {
            var repoName = "dockube-main";
            var token = "aa6f5c99f071abc0eb38c0c72a2c9e283b915ab7";
            var url = $"http://dockube:{token}@host.docker.internal:3000/dockube/dockube-main";
            //var url = $"http://host.docker.internal:3000/dockube/{repoName}";
            var server = "http://host.docker.internal:3000";

            CreateFileAndPushRepository(repoName, url);
            return Ok($"Added dummie file to {repoName}: {url}");
        }
        catch(Exception ex)
        {
            return Ok(ex.Message);
        }
    }

    public void StoreCredentials(string url, string token)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = "credential-cache store",
                RedirectStandardInput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            }
        };
        process.Start();
        process.StandardInput.WriteLine($"url={url}");
        process.StandardInput.WriteLine($"username={token}");
        process.StandardInput.WriteLine();
        process.WaitForExit();
    }

    public void CreateFileAndPushRepository(string repoName, string gogsServer)
    {
        // Initialize a new repository
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = $"clone {gogsServer}.git",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            }
        };
        process.Start();
        process.WaitForExit();

        Directory.SetCurrentDirectory(repoName);

        var filePath = "babar.md";
        
        System.IO.File.WriteAllText(filePath, "This is a new repository.");
        process.StartInfo.Arguments = $"add {filePath}";
        process.Start();
        process.WaitForExit();

        // Commit the changes
        process.StartInfo.Arguments = "commit -m \"Added file\"";
        process.Start();
        process.WaitForExit();

        // Push the commit to the remote origin
        process.StartInfo.Arguments = "push -v";
        process.Start();
        process.WaitForExit();
    }
}