using PainKiller.CommandPrompt.CoreLib.Core.BaseClasses;
using PainKiller.CommandPrompt.CoreLib.Core.Extensions;
using PainKiller.CommandPrompt.CoreLib.Metadata.Attributes;
using PainKiller.CommandPrompt.CoreLib.Modules.SecurityModule.Managers;

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
        input.TryGetOption(out string salt, "");
        var encryptionManager = new AESEncryptionManager(salt, Configuration.Core.Modules.Security.Encryption.KeySize);
        if (mode == "decrypt")
        {
            var plainText = DialogService.QuestionAnswerDialog("Input your string do decrypt:");
            var sharedSecret = DialogService.QuestionAnswerDialog("Input the shared secret:");
            var decryptedText = encryptionManager.DecryptString(plainText, sharedSecret);
            Console.WriteLine(decryptedText);
        }
        else
        {
            var plainText = DialogService.QuestionAnswerDialog("Input your string do encrypt:");
            var sharedSecret = DialogService.QuestionAnswerDialog("Input the shared secret:");
            var encryptedText = encryptionManager.EncryptString(plainText, sharedSecret);
            Console.WriteLine(encryptedText);
        }
        return Ok();
    }
}