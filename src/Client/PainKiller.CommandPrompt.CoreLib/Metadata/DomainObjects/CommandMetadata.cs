namespace PainKiller.CommandPrompt.CoreLib.Metadata.DomainObjects;

public class CommandMetadata
{
    public string Identifier { get; init; } = "";
    public string Description { get; init; } = "";
    public string[] Arguments { get; init; } = [];
    public string[] Quotes { get; init; } = [];
    public string[] Options { get; init; } = [];
    public string[] Examples { get; init; } = [];
    public string[] Suggestions { get; init; } = [];
}
