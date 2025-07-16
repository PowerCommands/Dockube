namespace PainKiller.DockubeClient.Commands;

[CommandDesign(     description:  "Dockube -  deletes content of a directory and copies content from a backup, hardcoded for testing purposes.")]
public class ResetCommand(string identifier) : ConsoleCommandBase<CommandPromptConfiguration>(identifier)
{
    public override RunResult Run(ICommandLineInput input)
    {
        var sourceDir = @"sourcedir";
        var targetDir = @"targetdir";
        
        if (Directory.Exists(targetDir))
        {
            foreach (var file in Directory.GetFiles(targetDir, "*", SearchOption.AllDirectories))
            {
                File.SetAttributes(file, FileAttributes.Normal);
            }

            Directory.Delete(targetDir, recursive: true);
        }
        else
        {
            Writer.WriteLine("You need to hardcode valid paths for source to restore from and target to restore to when testing something.");
            return Nok("Source or target directory does not exist. Please set valid paths in the code.");
        }

        CopyDirectory(sourceDir, targetDir);

        Writer.WriteLine($"Restored cloud-game directory from backup.");
        return Ok();
    }

    private void CopyDirectory(string sourceDir, string targetDir)
    {
        var source = new DirectoryInfo(sourceDir);
        var target = new DirectoryInfo(targetDir);

        Directory.CreateDirectory(target.FullName);
        
        foreach (var file in source.GetFiles())
        {
            string targetFilePath = Path.Combine(target.FullName, file.Name);
            file.CopyTo(targetFilePath, overwrite: true);
        }
        foreach (var subDir in source.GetDirectories())
        {
            string targetSubDir = Path.Combine(target.FullName, subDir.Name);
            CopyDirectory(subDir.FullName, targetSubDir);
        }
    }
}