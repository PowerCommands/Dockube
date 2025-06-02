namespace PainKiller.CommandPrompt.CoreLib.Modules.ShellModule.Services;
public static class IOService
{
    public static void CopyFolder(string sourceFolder, string destFolder)
    {
        if (destFolder.StartsWith(sourceFolder, StringComparison.OrdinalIgnoreCase))
        {
            ConsoleService.Writer.WriteError("Error: Destination folder is a subdirectory of the source folder.", nameof(CopyFolder));
            return;
        }

        if (!Directory.Exists(destFolder))
            Directory.CreateDirectory(destFolder);

        var files = Directory.GetFiles(sourceFolder);
        foreach (var file in files)
        {
            var name = Path.GetFileName(file);
            var dest = Path.Combine(destFolder, name);
            File.Copy(file, dest, overwrite: true);
        }

        var folders = Directory.GetDirectories(sourceFolder);
        foreach (var folder in folders)
        {
            var name = Path.GetFileName(folder);
            var dest = Path.Combine(destFolder, name);
            CopyFolder(folder, dest);
        }
    }
}