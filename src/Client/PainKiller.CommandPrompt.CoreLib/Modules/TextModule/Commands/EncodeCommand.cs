using PainKiller.CommandPrompt.CoreLib.Core.BaseClasses;
using System.Text;
using System.Web;
using PainKiller.CommandPrompt.CoreLib.Metadata.Attributes;
namespace PainKiller.CommandPrompt.CoreLib.Modules.TextModule.Commands;

[CommandDesign("Encode text to a specified format", options: ["base64", "url", "html"], examples: ["encode base64 Hello World"])]
public class EncodeCommand(string identifier) : ConsoleCommandBase<ApplicationConfiguration>(identifier)
{
    public override RunResult Run(ICommandLineInput input)
    {
        var text = string.Join(" ", input.Arguments);

        if (string.IsNullOrWhiteSpace(text))
        {
            Writer.WriteLine("No text was entered.");
            return Nok("No text captured.");
        }

        var encodingType = input.Options.Keys.FirstOrDefault() ?? "base64";

        try
        {
            string encoded = encodingType switch
            {
                "base64" => Convert.ToBase64String(Encoding.UTF8.GetBytes(text)),
                "url" => HttpUtility.UrlEncode(text),
                "html" => HttpUtility.HtmlEncode(text),
                _ => throw new ArgumentException("Unsupported encoding type.")
            };

            Writer.WriteLine($"Encoded ({encodingType}): {encoded}");
            Writer.WriteLine("Result copied to Clipboard");
            TextCopy.ClipboardService.SetText(encoded);
            return Ok();
        }
        catch (Exception ex)
        {
            Writer.WriteLine($"Error: {ex.Message}");
            return Nok("Encoding failed.");
        }
    }
}
