using PainKiller.CommandPrompt.CoreLib.Core.Enums;
using PainKiller.CommandPrompt.CoreLib.Modules.InfoPanelModule.Services;
using PainKiller.CommandPrompt.CoreLib.Modules.ShellModule.Services;

namespace PainKiller.DockubeClient.Commands;

[CommandDesign(     description: "Dockube -  Backup manifests, templates and certificates",
                      arguments: ["view"],
                    suggestions: ["view"],
                       examples: ["//Backup manifests, templates and certificates","backup"])]
public class BackupCommand(string identifier) : ConsoleCommandBase<CommandPromptConfiguration>(identifier)
{
    public override RunResult Run(ICommandLineInput input)
    {
        var manifestsSourcePath = Path.Combine(AppContext.BaseDirectory, Configuration.Dockube.ManifestsPath);
        var certificatesSourcePath = Path.Combine(AppContext.BaseDirectory, Configuration.Dockube.Ssl.Output);
        var templatesSourcePath = Path.Combine(AppContext.BaseDirectory, Configuration.Dockube.TemplatesPath);

        var safeTimeStamp = DateTime.Now.ToString("yyyyMMddHHmmss");
        var backupPath = Path.Combine(AppContext.BaseDirectory, Configuration.Dockube.BackupPath, $"backup{safeTimeStamp}");

        if (input.Arguments.FirstOrDefault() == "view")
        {
            var backupDir = new DirectoryInfo(Path.Combine(AppContext.BaseDirectory, Configuration.Dockube.BackupPath));
            foreach (var dir in backupDir.GetDirectories().OrderByDescending(d => d.CreationTime)) Writer.WriteLine($"│   ├──{Emo.File.Icon()} {dir.Name}");
            ShellService.Default.OpenDirectory(backupDir.FullName);
            return Ok("Backup directory opened in file explorer.");
        }
        try
        {
            Directory.CreateDirectory(backupPath);

            var backupManifestsPath = Path.Combine(backupPath, "manifests");
            var backupCertificatesPath = Path.Combine(backupPath, "certificates");
            var backupTemplatesPath = Path.Combine(backupPath, "templates");

            CopyDirectory(manifestsSourcePath, backupManifestsPath);
            CopyDirectory(certificatesSourcePath, backupCertificatesPath);
            CopyDirectory(templatesSourcePath, backupTemplatesPath);

            Writer.WriteSuccessLine( $"Backup completed to: {backupPath}");
            InfoPanelService.Instance.Update();
            return Ok();
        }
        catch (Exception ex)
        {
            Writer.WriteError(nameof(Run), $"Backup failed: {ex.Message}");
            return Nok();
        }
    }
    private void CopyDirectory(string sourceDir, string targetDir)
    {
        if (!Directory.Exists(sourceDir))
            return;

        Directory.CreateDirectory(targetDir);

        foreach (var file in Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories))
        {
            var relativePath = Path.GetRelativePath(sourceDir, file);
            var targetFilePath = Path.Combine(targetDir, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(targetFilePath)!);
            File.Copy(file, targetFilePath, overwrite: true);
        }
    }
}