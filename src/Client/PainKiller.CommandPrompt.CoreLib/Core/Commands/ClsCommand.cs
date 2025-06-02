using PainKiller.CommandPrompt.CoreLib.Core.BaseClasses;
using PainKiller.CommandPrompt.CoreLib.Metadata.Attributes;

namespace PainKiller.CommandPrompt.CoreLib.Core.Commands;


[CommandDesign("Clears the console window", examples: ["//Clear the console", "cls"])]
public class ClsCommand(string identifier) : ConsoleCommandBase<ApplicationConfiguration>(identifier)
{
    public override RunResult Run(ICommandLineInput input)
    {
        Writer.Clear();
        return Ok();
    }
}