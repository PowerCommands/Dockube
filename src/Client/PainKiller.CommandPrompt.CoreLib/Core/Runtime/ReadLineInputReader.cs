using PainKiller.ReadLine;
using PainKiller.ReadLine.Contracts;

namespace PainKiller.CommandPrompt.CoreLib.Core.Runtime;

public class ReadLineInputReader : IUserInputReader
{
    public string ReadLine(string prompt = "") => ReadLineService.Service.Read(prompt);
}
