using PainKiller.CommandPrompt.CoreLib.Core.Managers;

namespace PainKiller.CommandPrompt.CoreLib.Core.DomainObjects;
public class FileChecksum(string fileName)
{
    public string Mde5Hash { get; } = ChecksumManager.CalculateMd5ForFile(fileName);
}