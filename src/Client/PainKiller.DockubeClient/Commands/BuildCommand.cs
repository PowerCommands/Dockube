using Microsoft.Extensions.Logging;
using PainKiller.CommandPrompt.CoreLib.Logging.Services;
using PainKiller.CommandPrompt.CoreLib.Modules.ShellModule.Contracts;
using PainKiller.CommandPrompt.CoreLib.Modules.ShellModule.DomainObjects;
using PainKiller.CommandPrompt.CoreLib.Modules.ShellModule.Services;

namespace PainKiller.DockubeClient.Commands;

[CommandDesign(description: "Dockube -  Clone an existing Docker repo, build your own version with ARM64 support (default).",
                   options: ["git", "platform","publish","skipBuild","dockerfile"],
                  examples: ["//Build your docker image of https://github.com/giongto35/cloud-game.git with name cloud-game", "build https://github.com/giongto35/cloud-game.git \"cloud-game\"",
                             "//Build your docker image of the local git repo D:\\repos\\theGame with name the-game and linux/amd64 support and push it to your dockerhub repository\n(publish requires you to add dockerHubUserName to the Dockube configuration and authenticate your user.)", "build git=D:\\repos\\theGame \"the-game\" --platform=linux/amd64 --publish"])]
public class BuildCommand(string identifier) : ConsoleCommandBase<CommandPromptConfiguration>(identifier)
{
    private readonly ILogger<BuildCommand> _logger = LoggerProvider.CreateLogger<BuildCommand>();
    public override RunResult Run(ICommandLineInput input)
    {
        var url = input.Arguments.FirstOrDefault();
        var imageName = input.Quotes.FirstOrDefault();
        input.TryGetOption(out var platform, "linux/arm64");
        input.TryGetOption(out var dockerfile, "Dockerfile");
        input.TryGetOption(out var publish, false);
        input.TryGetOption(out var skipBuild, false);
        if (string.IsNullOrWhiteSpace(url) || string.IsNullOrEmpty(imageName)) return Nok("You must provide a repository URL and an image name to clone and build.");

        var imageTag = $"{DateTime.UtcNow:yyyy-MM-dd}{input.GetOptionValue("publish")}-{platform.Split('/').Last()}";
        var localTag = $"dockube/{imageName}:{imageTag}";    

        var remoteTag = $"{Configuration.Dockube.DockerHubUserName}/{imageName}:{imageTag}";

        var gitRepository = input.GetOptionValue("git");
        ITemporaryDirectory tempDir = string.IsNullOrWhiteSpace(gitRepository) ? new TemporaryDirectory() : new LocalDirectory(gitRepository);
        using (tempDir)
        {
            var dir = new DirectoryInfo(tempDir.Path);
            if (dir.GetFiles(searchPattern: "", SearchOption.AllDirectories).Length == 0)
            {
                Writer.WriteLine($"Begin Clone repo: {url} to {tempDir.Path} ...");
                CloneRepo(url, tempDir.Path);
                _logger.LogInformation("Cloned repository to temporary directory: {TempDirectory}", tempDir.Path);
            }
            if (!skipBuild)
            {
                Writer.WriteLine($"{nameof(EnsureBuildXBuilder)} ...");
                EnsureBuildXBuilder(nameof(DockubeClient));
                Writer.WriteLine($"Build image {localTag} Platform: {platform} ...");
                BuildImage(tempDir.Path, localTag, dockerfile, platform);
            }

            if (!publish) return Ok();
            Writer.WriteLine($"Push image {localTag} to {remoteTag}...");
            TagAndPushImage(localTag, remoteTag);
        }
        return Ok();
    }
    private void CloneRepo(string repoUrl, string tempDirectory)
    {
        // Ensure Git does not rewrite line endings (prevents CRLF issues in scripts)
        var configCmd = "config --global core.autocrlf input";
        ShellService.Default.StartInteractiveProcess("git", configCmd);

        var cloneCmd = $"clone {repoUrl} \"{tempDirectory}\"";
        var response = ShellService.Default.StartInteractiveProcess("git", cloneCmd);
        Writer.WriteLine(response);
    }
    private void EnsureBuildXBuilder(string builderName)
    {
        var listResult = ShellService.Default.StartInteractiveProcess("docker", "buildx ls");
        if (!listResult.Contains(builderName))
        {
            _logger.LogInformation("Creating new buildx builder: {BuilderName}", builderName);
            ShellService.Default.RunTerminalUntilUserQuits("docker", $"buildx create --use --name {builderName}");
            ShellService.Default.RunTerminalUntilUserQuits("docker", $"buildx inspect {builderName} --bootstrap");
        }
        else
        {
            _logger.LogInformation("Using existing buildx builder: {BuilderName}", builderName);
            ShellService.Default.RunTerminalUntilUserQuits("docker", $"buildx use {builderName}");
        }
    }
    private void BuildImage(string directory, string imageName, string dockerfile, string platform = "linux/arm64")
    {
        _logger.LogInformation("Building Docker image: {ImageName} from {Directory} for {Platform}", imageName, directory, platform);
        var dockerfilePath = Path.Combine(directory, dockerfile);

        var args = $"buildx build --no-cache --platform {platform} -t {imageName} --load -f \"{dockerfilePath}\" \"{directory}\"";
        ShellService.Default.RunTerminalUntilUserQuits("docker", args);
    }
    private void TagAndPushImage(string sourceTag, string destinationTag)
    {
        _logger.LogInformation("Tagging image {Source} as {Destination}", sourceTag, destinationTag);

        ShellService.Default.RunTerminalUntilUserQuits("docker", $"tag {sourceTag} {destinationTag}");

        _logger.LogInformation("Pushing image {Destination}", destinationTag);
        ShellService.Default.RunTerminalUntilUserQuits("docker", $"push {destinationTag}");
    }
}