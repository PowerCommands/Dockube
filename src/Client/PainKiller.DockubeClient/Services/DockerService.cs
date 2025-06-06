using Microsoft.Extensions.Logging;
using PainKiller.CommandPrompt.CoreLib.Logging.Services;
using PainKiller.CommandPrompt.CoreLib.Modules.ShellModule.Services;
using System.Diagnostics;

namespace PainKiller.DockubeClient.Services;

public class DockerService : IDockerService
{
    private DockerService() { }
    public static IDockerService Default => Instance.Value;
    private static readonly Lazy<IDockerService> Instance = new(() => new DockerService());
    private readonly ILogger<DockerService> _logger = LoggerProvider.CreateLogger<DockerService>();
    
    public string Version { get; private set; } = string.Empty;

    public string EnsureDockerRunning()
    {
        _logger.LogDebug("Checking Docker installation...");
        var dockerExePath = FindDockerExecutableInPath();

        if (!IsDockerDaemonRunning(dockerExePath))
        {
            _logger.LogInformation("Docker daemon is not running. Attempting to start Docker Desktop...");
            StartDockerDesktop();
            WaitForDockerDaemon(dockerExePath);
        }
        else
        {
            _logger.LogDebug("Docker daemon is already running.");
        }
        var version = GetDockerVersion(dockerExePath);
        _logger.LogInformation($"Docker version: {version}");
        return version;
    }
    private bool IsDockerDaemonRunning(string dockerExe)
    {
        try
        {
            var result = ShellService.Default.StartInteractiveProcess(dockerExe, "info");
            if (string.IsNullOrWhiteSpace(result)) return false;

            var lines = result.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            var hasServerSection = lines.Any(line => line.TrimStart().StartsWith("Server:", StringComparison.OrdinalIgnoreCase));
            var hasServerVersion = lines.Any(line => line.TrimStart().StartsWith("Version:", StringComparison.OrdinalIgnoreCase));

            return hasServerSection && hasServerVersion;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to query Docker daemon.");
            return false;
        }
    }
    private void StartDockerDesktop()
    {
        var dockerExe = FindDockerExecutableInPath();
        var dockerBinDir = Path.GetDirectoryName(dockerExe);
        if (dockerBinDir == null) throw new InvalidOperationException("Failed to resolve docker.exe path.");

        // Gå upp en nivå till "resources"
        var dockerResources = Path.GetFullPath(Path.Combine(dockerBinDir, ".."));
        var desktopPath = Path.Combine(dockerResources, "Docker Desktop.exe");

        if (File.Exists(desktopPath))
        {
            _logger.LogInformation($"Starting Docker Desktop from inferred path: {desktopPath}");
            ShellService.Default.Execute(desktopPath);
        }
        else
        {
            const string fallback = @"C:\Program Files\Docker\Docker\Docker Desktop.exe";
            if (File.Exists(fallback))
            {
                _logger.LogWarning("Inferred Docker Desktop path not found. Falling back to default.");
                ShellService.Default.Execute(fallback);
            }
            else
            {
                var msg = $"Could not find Docker Desktop at: {desktopPath} or fallback: {fallback}";
                _logger.LogError(msg);
                throw new FileNotFoundException(msg);
            }
        }
    }

    private void WaitForDockerDaemon(string dockerExe, int timeoutSeconds = 60)
    {
        _logger.LogDebug($"Waiting for Docker daemon to start (timeout: {timeoutSeconds} seconds)...");
        var stopwatch = Stopwatch.StartNew();
        while (stopwatch.Elapsed.TotalSeconds < timeoutSeconds)
        {
            if (IsDockerDaemonRunning(dockerExe))
            {
                _logger.LogDebug("Docker daemon is now running.");
                return;
            }
            Thread.Sleep(1000);
        }

        var msg = "Docker daemon did not start within the expected time.";
        _logger.LogError(msg);
        throw new TimeoutException(msg);
    }
    private string GetDockerVersion(string dockerExe)
    {
        var version = ShellService.Default.StartInteractiveProcess(dockerExe, "--version");
        if (string.IsNullOrWhiteSpace(version))
        {
            var msg = "Failed to retrieve Docker version.";
            _logger.LogError(msg);
            throw new Exception(msg);
        }
        Version = version.Trim();
        return Version;
    }
    private string FindDockerExecutableInPath()
    {
        var pathEnv = Environment.GetEnvironmentVariable("PATH") ?? "";
        var paths = pathEnv.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries);

        foreach (var path in paths)
        {
            var fullPath = Path.Combine(path, "docker.exe");
            if (File.Exists(fullPath))
            {
                _logger.LogDebug($"Found docker.exe at: {fullPath}");
                return fullPath;
            }
        }

        var msg = "Could not find 'docker.exe' in PATH.";
        _logger.LogError(msg);
        throw new FileNotFoundException(msg);
    }
}