using System.Text.RegularExpressions;
using PainKiller.CommandPrompt.CoreLib.Modules.InfoPanelModule.Services;
using PainKiller.CommandPrompt.CoreLib.Modules.ShellModule.Services;

namespace PainKiller.DockubeClient.Managers;

public class KubeEnvironmentManager
{
    private readonly string _kubeDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".kube");
    private string CurrentConfig => Path.Combine(_kubeDir, "config");

    public void SwitchEnvironment(string environmentName)
    {
        var sourceConfig = Path.Combine(_kubeDir, $"config-{environmentName}.yaml");
        if (!File.Exists(sourceConfig))
            throw new FileNotFoundException($"Missing kubeconfig file: {sourceConfig}");

        BackupCurrentConfig();
        File.Copy(sourceConfig, CurrentConfig, overwrite: true);
        InfoPanelService.Instance.Update();
        Console.WriteLine($"✅ Switched Kubernetes environment to '{environmentName}'");
    }
    public static string GetTarget()
    {
        var response = ShellService.Default.StartInteractiveProcess("kubectl", "get nodes");
        var version = response.Contains("docker-desktop",StringComparison.OrdinalIgnoreCase) ? "Docker Desktop" : "k3s";
        return version;
    }
    public static string GetVersion()
    {
        var response = ShellService.Default.StartInteractiveProcess("kubectl", "version");
        var lines = response.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        var parts = lines.Select(line =>
        {
            var clean = line.Replace(" Version", "").Trim();
            return clean;
        });
        var version = string.Join(" ", parts);
        return version;
    }
    public static string GetHelmVersion()
    {
        var response = ShellService.Default.StartInteractiveProcess("helm", "version");
        var match = Regex.Match(response, @"Version:\s*""(?<version>v[0-9]+\.[0-9]+\.[0-9]+)""");
        return match.Success ? match.Groups["version"].Value : "unknown";
    }
    private void BackupCurrentConfig()
    {
        if (!File.Exists(CurrentConfig))
        {
            Console.WriteLine("⚠️  No existing config to backup.");
            return;
        }

        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var backupFile = Path.Combine(_kubeDir, $"backup_{timestamp}.yaml");

        File.Copy(CurrentConfig, backupFile);
        Console.WriteLine($"📁 Backup created: {backupFile}");
    }
}