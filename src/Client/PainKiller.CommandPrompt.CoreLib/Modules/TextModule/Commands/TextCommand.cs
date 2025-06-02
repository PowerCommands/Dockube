using PainKiller.CommandPrompt.CoreLib.Core.BaseClasses;
using PainKiller.CommandPrompt.CoreLib.Metadata.Attributes;
using PainKiller.CommandPrompt.CoreLib.Modules.TextModule.Utility;

namespace PainKiller.CommandPrompt.CoreLib.Modules.TextModule.Commands;

[CommandDesign(
    description: "Captures multiline text input from the user. The command reads text line by line until the user presses the ESC key, signaling the end of input. The captured text is then displayed in the console.",
       examples: ["text"]
)]
public class TextCommand(string identifier) : ConsoleCommandBase<ApplicationConfiguration>(identifier)
{
    public override RunResult Run(ICommandLineInput input)
    {
        var resultText = TextInputHelper.CaptureText();

        if (string.IsNullOrWhiteSpace(resultText))
        {
            Writer.WriteLine("No text was entered.");
            return Nok("No text captured.");
        }
        Writer.WriteLine(resultText);
        return Ok();
    }
}
