using PainKiller.CommandPrompt.CoreLib.Core.BaseClasses;
using PainKiller.CommandPrompt.CoreLib.Core.Contracts;
using PainKiller.CommandPrompt.CoreLib.Core.DomainObjects;
using PainKiller.CommandPrompt.CoreLib.Metadata.Attributes;
using PainKiller.DockubeClient.Configuration;

namespace PainKiller.DockubeClient.Commands;

[CommandDesign(     description: "Demo command", 
                        options: ["Apple","Pear"],
                       examples: ["//Demo of Command Prompts","demo"])]
public class DemoCommand(string identifier) : ConsoleCommandBase<CommandPromptConfiguration>(identifier)
{
    public override RunResult Run(ICommandLineInput input)
    {
        Writer.WriteHeadLine($"Hello there {Environment.UserName}");
        Writer.WriteLine();
        Writer.WriteSuccessLine("Congrats to your first command");
        Writer.WriteDescription("Did you input something?", $"{string.Join(",", input.Arguments)} {string.Join(",", input.Quotes)} {string.Join(",", input.Options.Keys)}");
        Writer.WriteLine();
        Writer.WriteUrl("https://github.com/PowerCommands/CommandPrompt");
        return Ok();
    }
}