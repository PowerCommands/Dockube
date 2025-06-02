using System.Reflection;
using PainKiller.CommandPrompt.CoreLib.Metadata.Attributes;
using PainKiller.CommandPrompt.CoreLib.Metadata.DomainObjects;

namespace PainKiller.CommandPrompt.CoreLib.Metadata.Extensions;
public static class CommandMetadataExtensions
{
    public static CommandMetadata? GetMetadata(this IConsoleCommand command)
    {
        var type = command.GetType();
        var attr = type.GetCustomAttribute<CommandDesignAttribute>();
        if (attr == null) return null;

        return new CommandMetadata
        {
            Identifier = command.Identifier,
            Description = attr.Description,
            Arguments = attr.Arguments,
            Quotes = attr.Quotes,
            Options = attr.Options,
            Examples = attr.Examples,
            Suggestions = attr.Suggestions
        };
    }
}
