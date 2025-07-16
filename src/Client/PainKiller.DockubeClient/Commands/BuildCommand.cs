using Microsoft.Extensions.Logging;
using PainKiller.CommandPrompt.CoreLib.Logging.Services;
using PainKiller.CommandPrompt.CoreLib.Modules.ShellModule.Contracts;
using PainKiller.CommandPrompt.CoreLib.Modules.ShellModule.DomainObjects;
using PainKiller.CommandPrompt.CoreLib.Modules.ShellModule.Services;

namespace PainKiller.DockubeClient.Commands;

[CommandDesign(description: "Dockube -  Clone an existing Docker repo, build your own version with ARM64 support.",
                   options: ["git", "platform","publish","skipBuild"],
                  examples: ["//Build your docker image of https://github.com/giongto35/cloud-game.git with name cloud-game", "build https://github.com/giongto35/cloud-game.git \"cloud-game\""])]
public class BuildCommand(string identifier) : ConsoleCommandBase<CommandPromptConfiguration>(identifier)
{
    private readonly ILogger<BuildCommand> _logger = LoggerProvider.CreateLogger<BuildCommand>();
    public override RunResult Run(ICommandLineInput input)
    {
        var url = input.Arguments.FirstOrDefault();
        var imageName = input.Quotes.FirstOrDefault();
        input.TryGetOption(out var platform, "linux/arm64");
        input.TryGetOption(out var publish, false);
        input.TryGetOption(out var skipBuild, false);
        if (string.IsNullOrWhiteSpace(url) || string.IsNullOrEmpty(imageName)) return Nok("You must provide a repository URL and an image name to clone and build.");

        var dateTag = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var localTag = $"dockube/{imageName}:{dateTag}";
        var remoteTag = $"{Configuration.Dockube.DockerHubUserName}/{imageName}:{dateTag}";

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
                if (platform == "linux/arm64")
                {
                    Writer.WriteLine($"Adjust makefile for arm64.");
                    PatchMakefileForArm64(tempDir.Path);
                }
                Writer.WriteLine($"{nameof(EnsureBuildXBuilder)} ...");
                EnsureBuildXBuilder(nameof(DockubeClient));
                Writer.WriteLine($"Build image {localTag} Platform: {platform} ...");
                BuildImage(tempDir.Path, localTag, platform);
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
    private void BuildImage(string directory, string imageName, string platform = "linux/arm64")
    {
        _logger.LogInformation("Building Docker image: {ImageName} from {Directory} for {Platform}", imageName, directory, platform);

        var args = $"buildx build --platform {platform} -t {imageName} --load \"{directory}\"";
        ShellService.Default.RunTerminalUntilUserQuits("docker", args);
    }
    private void TagAndPushImage(string sourceTag, string destinationTag)
    {
        _logger.LogInformation("Tagging image {Source} as {Destination}", sourceTag, destinationTag);

        ShellService.Default.RunTerminalUntilUserQuits("docker", $"tag {sourceTag} {destinationTag}");

        _logger.LogInformation("Pushing image {Destination}", destinationTag);
        ShellService.Default.RunTerminalUntilUserQuits("docker", $"push {destinationTag}");
    }

    private void PatchMakefileForArm64(string projectDir)
    {
        var makefilePath = Path.Combine(projectDir, "Makefile");
        if (!File.Exists(makefilePath)) return;

        var lines = File.ReadAllLines(makefilePath).ToList();

        for (int i = 0; i < lines.Count; i++)
        {
            var trimmed = lines[i].TrimStart();

            // Modifiera CGO_CFLAGS och CGO_LDFLAGS
            if (trimmed.StartsWith("CGO_CFLAGS=") && !lines[i].Contains("-march=armv8-a"))
            {
                lines[i] = "CGO_CFLAGS='-g -O3 -march=armv8-a'";
            }
            if (trimmed.StartsWith("CGO_LDFLAGS=") && !lines[i].Contains("-march=armv8-a"))
            {
                lines[i] = "CGO_LDFLAGS='-g -O3 -march=armv8-a'";
            }

            // Injicera GOARCH=arm64 CGO_ENABLED=1 före go build
            if (trimmed.StartsWith("go build") && !trimmed.Contains("GOARCH="))
            {
                lines[i] = "\tGOARCH=arm64 CGO_ENABLED=1 " + trimmed;
            }

            // Injicera GOARCH=arm64 på CGO-byggkommandon
            if ((trimmed.StartsWith("CGO_CFLAGS=") || trimmed.StartsWith("CGO_LDFLAGS=")) &&
                i + 1 < lines.Count &&
                lines[i + 1].TrimStart().StartsWith("go build") &&
                !lines[i + 1].Contains("GOARCH="))
            {
                lines[i + 1] = "\tGOARCH=arm64 CGO_ENABLED=1 " + lines[i + 1].TrimStart();
            }

            // Fixa recept-indentering: TAB istället för spaces
            if (i > 0 && lines[i - 1].TrimEnd().EndsWith(":") &&
                lines[i].StartsWith("    ") && !lines[i].StartsWith("\t"))
            {
                lines[i] = "\t" + lines[i].TrimStart();
            }
        }

        File.WriteAllLines(makefilePath, lines);
    }



}