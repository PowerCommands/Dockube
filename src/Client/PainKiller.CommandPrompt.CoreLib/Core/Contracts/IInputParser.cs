namespace PainKiller.CommandPrompt.CoreLib.Core.Contracts;

public interface IInputParser
{
    ICommandLineInput Parse(string rawInput);
}