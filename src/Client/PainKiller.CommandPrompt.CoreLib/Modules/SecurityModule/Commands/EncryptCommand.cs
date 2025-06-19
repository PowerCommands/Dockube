using PainKiller.CommandPrompt.CoreLib.Core.BaseClasses;
using PainKiller.CommandPrompt.CoreLib.Core.Extensions;
using PainKiller.CommandPrompt.CoreLib.Metadata.Attributes;
using PainKiller.CommandPrompt.CoreLib.Modules.SecurityModule.Services;

namespace PainKiller.CommandPrompt.CoreLib.Modules.SecurityModule.Commands;

[CommandDesign(description: "Encrypt one single message with temporary salt and shared secret",
                   options: ["salt"],           
               suggestions: ["encrypt", "decrypt"],
                  examples: ["//Encrypt a message"])]
public class EncryptCommand(string identifier) : ConsoleCommandBase<ApplicationConfiguration>(identifier)
{
    public override RunResult Run(ICommandLineInput input)
    {
        var mode = this.GetSuggestion(input.Arguments.FirstOrDefault(), "encrypt");
        if (mode == "decrypt")
        {
            var plainText = DialogService.QuestionAnswerDialog("Input your string do decrypt:");
            var decryptedText = EncryptionService.Service.DecryptString(plainText);
            Console.WriteLine(decryptedText);
        }
        else
        {
            var plainText = DialogService.QuestionAnswerDialog("Input your string do encrypt:");
            var encryptedText = EncryptionService.Service.EncryptString(plainText);
            Console.WriteLine(encryptedText);
        }
        return Ok();
    }
}