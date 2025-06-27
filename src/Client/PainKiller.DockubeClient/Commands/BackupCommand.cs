namespace PainKiller.DockubeClient.Commands;

[CommandDesign(     description: "Backup manifests and certificates", 
                       examples: ["//Backup manifests and certificates","backup"])]
public class BackupCommand(string identifier) : ConsoleCommandBase<CommandPromptConfiguration>(identifier)
{
    public override RunResult Run(ICommandLineInput input)
    {
        var manifestsSourcePath = Path.Combine(AppContext.BaseDirectory, Configuration.Dockube.ManifestsPath);
        var certificatesSourcePath = Path.Combine(AppContext.BaseDirectory, Configuration.Dockube.Ssl.Output);

        var safeTimeStamp = DateTime.Now.ToString("yyyyMMddHHmmss");
        var backupPath = Path.Combine(AppContext.BaseDirectory, Configuration.Dockube.BackupPath, $"backup{safeTimeStamp}");

        try
        {
            Directory.CreateDirectory(backupPath);

            var backupManifestsPath = Path.Combine(backupPath, "manifests");
            var backupCertificatesPath = Path.Combine(backupPath, "certificates");

            CopyDirectory(manifestsSourcePath, backupManifestsPath);
            CopyDirectory(certificatesSourcePath, backupCertificatesPath);

            Writer.WriteSuccessLine( $"Backup completed to: {backupPath}");
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