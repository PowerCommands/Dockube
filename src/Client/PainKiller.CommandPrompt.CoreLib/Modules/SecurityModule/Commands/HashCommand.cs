using PainKiller.CommandPrompt.CoreLib.Core.BaseClasses;
using PainKiller.CommandPrompt.CoreLib.Metadata.Attributes;
using PainKiller.CommandPrompt.CoreLib.Modules.SecurityModule.Extensions;

namespace PainKiller.CommandPrompt.CoreLib.Modules.SecurityModule.Commands;

[CommandDesign(description: "Hash input with BCrypt",
                  examples: ["hash"])]
public class HashCommand(string identifier) : ConsoleCommandBase<ApplicationConfiguration>(identifier)
{
    public override RunResult Run(ICommandLineInput input)
    {
        var text = $"{input.Arguments.FirstOrDefault()}";
        var bCrypt = text.GetBCrypt();
        Console.WriteLine(bCrypt);
        return Ok();
    }
}