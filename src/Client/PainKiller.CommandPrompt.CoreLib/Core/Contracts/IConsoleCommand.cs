using PainKiller.CommandPrompt.CoreLib.Core.DomainObjects;

namespace PainKiller.CommandPrompt.CoreLib.Core.Contracts;

public interface IConsoleCommand
{
    string Identifier { get; }
    RunResult Run(ICommandLineInput input);
    void OnInitialized();
}