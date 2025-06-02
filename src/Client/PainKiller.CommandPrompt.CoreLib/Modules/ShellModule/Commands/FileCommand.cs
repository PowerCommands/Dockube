using PainKiller.CommandPrompt.CoreLib.Core.BaseClasses;
using PainKiller.CommandPrompt.CoreLib.Core.Events;
using PainKiller.CommandPrompt.CoreLib.Core.Extensions;
using PainKiller.CommandPrompt.CoreLib.Metadata.Attributes;
using PainKiller.CommandPrompt.CoreLib.Modules.ShellModule.Services;
using PainKiller.ReadLine.Managers;

namespace PainKiller.CommandPrompt.CoreLib.Modules.ShellModule.Commands;

[CommandDesign(
    description: "Handle basic file actions such as read, write, delete, copy, move\n" +
                 "You can use tab to traverse through current directory, use cd and dir command to change current directory or see its content.\n" +
                 "Remember that filename containing white spaces must be surrounded with quotation marks.",
    options: ["read", "write", "delete", "open", "properties", "copy", "move", "confirm", "overwrite"],
    arguments: ["fileName"],
    examples: ["//Read a file in current working directory (use dir command to see current directory, cd command to change directory)","file filename.txt --read",
               "//View properties", "file filename.txt --properties"])]
public class FileCommand : ConsoleCommandBase<ApplicationConfiguration>
{
    private void OnWorkingDirectoryChanged(WorkingDirectoryChangedEventArgs e) => UpdateSuggestions(e.NewWorkingDirectory);
    private ICommandLineInput _input;

#pragma warning disable CS8618, CS9264
    public FileCommand(string identifier) : base(identifier) => EventBusService.Service.Subscribe<WorkingDirectoryChangedEventArgs>(OnWorkingDirectoryChanged);
#pragma warning restore CS8618, CS9264

    public override RunResult Run(ICommandLineInput input)
    {
        _input = input;
        var inputs = input.Raw.Split(' ');

        if (inputs.Length < 2) return Nok("You must provide a file name and at least one option flag.");
        var path = inputs[1].Contains("\"") ? input.Quotes.First().Replace("\"", "") : inputs[1];
        if (!path.Contains('\\')) path = Path.Combine(Environment.CurrentDirectory, path);

        // Handle each option using Options from ICommandLineInput
        if (input.HasOption("read")) return ReadFile(path);
        if (input.HasOption("write")) return WriteFile(path);
        if (input.HasOption("delete")) return DeleteFile(path);
        if (input.HasOption("copy")) return CopyFile(path);
        if (input.HasOption("move")) return MoveFile(path);
        if (input.HasOption("open")) return OpenFile(path);
        return ShowFileInfo(path);
    }

    private void UpdateSuggestions(string newWorkingDirectory)
    {
        if (Directory.Exists(newWorkingDirectory))
        {
            var files = Directory.GetFiles(newWorkingDirectory)
                .Select(f => new FileInfo(f).Name)
                .ToArray();
            SuggestionProviderManager.AppendContextBoundSuggestions(Identifier, files.Select(e => e).ToArray());
        }
    }

    private RunResult ReadFile(string path)
    {
        if (!File.Exists(path)) return Nok($"{path} does not exist!");
        var content = File.ReadAllText(path);
        Writer.WriteLine(content);  // Normal output, no color
        return Ok();
    }

    private RunResult WriteFile(string path)
    {
        var content = _input.GetOptionValue("text");  // Simple text input to write to file
        File.WriteAllText(path, content);
        Writer.WriteSuccessLine($"File [{path}] successfully written.");  // Green for success
        EventBusService.Service.Publish(new WorkingDirectoryChangedEventArgs(Environment.CurrentDirectory));
        return Ok();
    }

    private RunResult DeleteFile(string path)
    {
        if (!File.Exists(path)) return Nok($"{path} does not exist!");

        var confirm = _input.HasOption("confirm");
        if (confirm)
        {
            var dialogResponse = DialogService.YesNoDialog($"Do you want to delete the file {path}?");
            if (!dialogResponse) return Ok();  // User chose not to delete
        }

        File.Delete(path);
        Writer.WriteSuccessLine($"The file [{path}] has successfully been deleted.");  // Green for success
        return Ok();
    }

    private RunResult CopyFile(string path)
    {
        var targetPath = _input.GetOptionValue("copy");
        if (!File.Exists(path)) return Nok($"{path} does not exist!");
        File.Copy(path, targetPath, overwrite: true);
        Writer.WriteSuccessLine($"File [{path}] successfully copied to [{targetPath}].");  // Green for success
        return Ok();
    }

    private RunResult MoveFile(string path)
    {
        var targetPath = _input.GetOptionValue("move");
        if (!File.Exists(path)) return Nok($"{path} does not exist!");
        File.Move(path, targetPath);
        Writer.WriteSuccessLine($"File [{path}] successfully moved to [{targetPath}].");  // Green for success
        return Ok();
    }

    private RunResult OpenFile(string path)
    {
        if (!File.Exists(path)) return Nok($"{path} does not exist!");
        ShellService.Default.OpenWithDefaultProgram(path);
        return Ok();
    }
    private RunResult ShowFileInfo(string path)
    {
        if (!File.Exists(path)) return Nok($"{path} does not exist!");

        var fileInfo = new FileInfo(path);
        Writer.WriteTable([new {fileInfo.Name, Path = fileInfo.FullName, Size = fileInfo.Length.GetDisplayFormattedFileSize(), Created = fileInfo.CreationTime.GetDisplayTimeSinceLastUpdate(), Modified = fileInfo.LastAccessTime.GetDisplayTimeSinceLastUpdate()}]);
        return Ok();
    }
}
