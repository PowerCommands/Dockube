namespace PainKiller.CommandPrompt.CoreLib.Core.Contracts;

public interface ICommandExecutor
{
    RunResult Execute(IConsoleCommand? command, ICommandLineInput commandLineInput);
}