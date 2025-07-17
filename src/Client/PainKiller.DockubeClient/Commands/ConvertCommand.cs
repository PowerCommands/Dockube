using System.Text;
using PainKiller.CommandPrompt.CoreLib.Core.Events;
using PainKiller.CommandPrompt.CoreLib.Core.Services;
using PainKiller.ReadLine.Managers;

namespace PainKiller.DockubeClient.Commands;

[CommandDesign(     description: "Dockube -  Convert a file to Unix/Linux line endings format.", 
                      arguments: ["<filename>"],
                       examples: ["//Convert a file to Unix/Linux line endings format.","entrypoint.sh"])]
public class ConvertCommand(string identifier) : ConsoleCommandBase<CommandPromptConfiguration>(identifier)
{
    private void OnWorkingDirectoryChanged(WorkingDirectoryChangedEventArgs e) => UpdateSuggestions(e.NewWorkingDirectory);
    public override void OnInitialized() => EventBusService.Service.Subscribe<WorkingDirectoryChangedEventArgs>(OnWorkingDirectoryChanged);

    public override RunResult Run(ICommandLineInput input)
    {
        var filePath = input.Arguments.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath)) return Nok("Please provide a valid file path.");
        try
        {
            var lines = File.ReadAllLines(filePath);
            var unixContent = string.Join('\n', lines); // Convert to LF
            File.WriteAllText(filePath, unixContent, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            Writer.WriteSuccessLine($"File '{filePath}' converted to Unix line endings.");
            return Ok($"File '{filePath}' converted to Unix line endings.");
        }
        catch (Exception ex)
        {
            return Nok($"Error: {ex.Message}");
        }
    }
    private void UpdateSuggestions(string newWorkingDirectory)
    {
        if (!Directory.Exists(newWorkingDirectory)) return;
        var files = Directory.GetFiles(newWorkingDirectory).Select(f => new FileInfo(f).Name).ToArray();
        SuggestionProviderManager.AppendContextBoundSuggestions(Identifier, files.Select(e => e).ToArray());
    }
}
