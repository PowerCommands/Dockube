namespace PainKiller.CommandPrompt.CoreLib.Metadata.Contracts;
public interface IMetadataRegistry : IMetadataRegistryReader
{
    void Register(IConsoleCommand command);
}
