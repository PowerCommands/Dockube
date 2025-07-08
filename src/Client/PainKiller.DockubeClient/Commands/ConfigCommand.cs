using PainKiller.CommandPrompt.CoreLib.Core.Enums;
using PainKiller.DockubeClient.Bootstrap;
using PainKiller.DockubeClient.Managers;

namespace PainKiller.DockubeClient.Commands;

[CommandDesign(description: "Dockube -  See detailed information about current configuration",
                  examples: ["//See detailed information about current configuration", "config"])]
public class ConfigCommand(string identifier) : ConsoleCommandBase<CommandPromptConfiguration>(identifier)
{
    public override RunResult Run(ICommandLineInput input)
    {
        Writer.Clear();
        Startup.ShowLogo(Configuration.Core);

        Writer.WriteHeadLine($"{Emo.Settings.Icon()} Configuration");
        Writer.WriteDescription($"├──{Emo.CirclePurple.Icon()} Domain ", Configuration.Dockube.DefaultDomain, noBorder: true);
        Writer.WriteDescription($"├──{Emo.Directory.Icon()} Manifests path ", Configuration.Dockube.ManifestsPath, noBorder: true);
        Writer.WriteDescription($"├──{Emo.Directory.Icon()} Templates path ", Configuration.Dockube.TemplatesPath, noBorder: true);
        var templatesDir = new DirectoryInfo(Path.Combine(AppContext.BaseDirectory, Configuration.Dockube.TemplatesPath));
        Writer.WriteDescription($"├──{Emo.Directory.Icon()} Backups path ", Configuration.Dockube.BackupPath, noBorder: true);
        var backupDir = new DirectoryInfo(Path.Combine(AppContext.BaseDirectory, Configuration.Dockube.BackupPath));
        foreach (var dir in backupDir.GetDirectories().OrderByDescending(d => d.CreationTime).Take(10)) Writer.WriteLine($"│   ├──{Emo.File.Icon()} {dir.Name}");
        
        Writer.WriteDescription($"├── Services ", "Status check", noBorder: true);
        foreach (var service in ServiceStatusManager.GetServicesStatus(Configuration.Dockube))
        {
            var start = service.Tsl ? "https://" : "http://";
            var url = $"{start}{service.Host}.{Configuration.Dockube.DefaultDomain}";
            var status = service.IsAvailable ? "✅" : "❌";
            Writer.WriteLine($"│   ├──{Emo.Package.Icon()} {service.Name} {url} {status}");
        }
        
        Writer.WriteDescription($"├── Open SSL ", "Settings", noBorder: true);
        Writer.WriteLine($"│   ├──{Emo.Shield.Icon()} Executable: {Configuration.Dockube.Ssl.ExecutablePath} Output:{Configuration.Dockube.Ssl.Output}");
        
        Writer.WriteDescription($"├── SSH ", $"Machines", noBorder: true);
        foreach (var ssh in Configuration.Dockube.Ssh) Writer.WriteLine($"│   ├──{Emo.Workspace.Icon()} {ssh.Name} {ssh.Host} Port:{ssh.Port}");

        var versionInfo = VersionInformationManager.GetVersionInformation();
        Writer.WriteDescription($"├── Environment ", $"Versions", noBorder: true);
        Writer.WriteDescription($"├──{Emo.CircleBlue.Icon()} Target ", versionInfo.CurrentEnvironment, noBorder: true);
        Writer.WriteDescription($"├──{Emo.CircleWhite.Icon()} Kubernetes ", versionInfo.KubeVersion, noBorder: true);
        Writer.WriteDescription($"├──{Emo.CircleGreen.Icon()} Open SSL ", versionInfo.SslVersion, noBorder: true);
        Writer.WriteDescription($"├──{Emo.CircleBrown.Icon()} Helm ", versionInfo.HelmVersion, noBorder: true);
        
        return Ok();
    }
}
