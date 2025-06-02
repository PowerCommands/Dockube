namespace PainKiller.CommandPrompt.CoreLib.Metadata.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class CommandDesignAttribute(string description, string[]? arguments = null, string[]? quotes = null, string[]? options = null, string[]? examples = null, string[]? suggestions = null) : Attribute
{
    public string Description { get; } = description;
    public string[] Arguments { get; } = arguments ?? [];
    public string[] Quotes { get; } = quotes ?? [];
    public string[] Options { get; } = options ?? [];
    public string[] Examples { get; } = examples ?? [];
    public string[] Suggestions { get; } = suggestions ?? [];
}