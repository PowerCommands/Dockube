using PainKiller.CommandPrompt.CoreLib.Metadata.DomainObjects;

namespace PainKiller.CommandPrompt.CoreLib.Metadata.Contracts;
public interface IMetadataRegistryReader
{
    CommandMetadata? Get(string identifier);
    IReadOnlyDictionary<string, CommandMetadata> All { get; }
}