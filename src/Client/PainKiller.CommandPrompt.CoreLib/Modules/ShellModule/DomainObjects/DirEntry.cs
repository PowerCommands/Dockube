namespace PainKiller.CommandPrompt.CoreLib.Modules.ShellModule.DomainObjects;
public class DirEntry
{
    public string Name { get; init; } = "";
    public string Type { get; init; } = "";
    public string Size { get; init; } = "";
    public long SizeInBytes { get; init; }
    public string Updated { get; init; } = "";
    public DateTime UpdatedTime { get; init; }
}