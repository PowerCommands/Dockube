using System.Text;
using System.Web;
using PainKiller.CommandPrompt.CoreLib.Core.BaseClasses;
using PainKiller.CommandPrompt.CoreLib.Metadata.Attributes;
namespace PainKiller.CommandPrompt.CoreLib.Modules.TextModule.Commands;

[CommandDesign("Decode text from a specified format", options: ["base64", "url", "html"], examples: ["decode base64 SGVsbG8gV29ybGQ="])]
public class DecodeCommand(string identifier) : ConsoleCommandBase<ApplicationConfiguration>(identifier)
{
    public override RunResult Run(ICommandLineInput input)
    {
        var encodedText = string.Join(" ", input.Arguments);
        if (string.IsNullOrWhiteSpace(encodedText))
        {
            Writer.WriteLine("No text was entered.");
            return Nok("No text captured.");
        }
        var encodingType = input.Options.Keys.FirstOrDefault() ?? "base64";
        try
        {
            string decoded = encodingType switch
            {
                "base64" => Encoding.UTF8.GetString(Convert.FromBase64String(encodedText)),
                "url" => HttpUtility.UrlDecode(encodedText),
                "html" => HttpUtility.HtmlDecode(encodedText),
                _ => throw new ArgumentException("Unsupported decoding type.")
            };

            Writer.WriteLine($"Decoded ({encodingType}): {decoded}");
            Writer.WriteLine("Result copied to Clipboard");
            TextCopy.ClipboardService.SetText(decoded);
            return Ok();
        }
        catch (Exception ex)
        {
            Writer.WriteLine($"Error: {ex.Message}");
            return Nok("Decoding failed.");
        }
    }
}