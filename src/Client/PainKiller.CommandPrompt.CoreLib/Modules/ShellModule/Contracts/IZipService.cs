using PainKiller.CommandPrompt.CoreLib.Modules.ShellModule.DomainObjects;

namespace PainKiller.CommandPrompt.CoreLib.Modules.ShellModule.Contracts;
public interface IZipService
{
    ZipResult ArchiveFilesInDirectory(string directoryPath, string archiveName, bool useTimestampSuffix = false, string filter = "*", string outputDirectory = "");
    bool ExtractZipFile(string zipFilePath, string extractToDirectory);
}