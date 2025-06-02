using PainKiller.CommandPrompt.CoreLib.Core.BaseClasses;
using PainKiller.CommandPrompt.CoreLib.Core.Events;
using PainKiller.CommandPrompt.CoreLib.Core.Extensions;
using PainKiller.CommandPrompt.CoreLib.Metadata.Attributes;
using PainKiller.CommandPrompt.CoreLib.Modules.ShellModule.DomainObjects;
using PainKiller.ReadLine.Managers;
using Spectre.Console;

namespace PainKiller.CommandPrompt.CoreLib.Modules.ShellModule.Commands;

[CommandDesign(
    description: "Change or view the current working directory",
    options: ["roaming", "startup", "root", "recent", "documents", "programs", "windows", "profile", "templates", "videos", "pictures", "music","modules","no-output"],
    arguments: ["Path or navigation command such as .. or \\"],
    examples:
    [
        "//View current working directory", "cd",
        "//Traverse down one directory", "cd ..",
        "//Change working directory", "cd C:\\ProgramData",
        "cd 'My Folder'",
        "cd --documents"
    ]
)]
public class CdCommand : ConsoleCommandBase<ApplicationConfiguration>
{
    private void OnWorkingDirectoryChanged(WorkingDirectoryChangedEventArgs e) => UpdateSuggestions(e.NewWorkingDirectory);
    public CdCommand(string identifier) : base(identifier) => EventBusService.Service.Subscribe<WorkingDirectoryChangedEventArgs>(OnWorkingDirectoryChanged);

    public override RunResult Run(ICommandLineInput input)
    {
        var path = Environment.CurrentDirectory;
        var arg = string.Join(" ", input.Arguments).Trim();
        var lowerArgs = input.Options.Select(o => o.Key.ToLower()).ToList();

        if (arg == "\\") path = Directory.GetDirectoryRoot(path);
        else if (arg == "..") path = Path.GetDirectoryName(path) ?? path;
        else if (!string.IsNullOrWhiteSpace(arg))
        {
            // Hantera sökvägar med mellanslag korrekt
            if (Path.IsPathRooted(arg))
            {
                path = arg;
            }
            else
            {
                path = Path.Combine(path, arg);
            }
        }
        if (lowerArgs.Contains("roaming"))
            path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Configuration.Core.RoamingDirectory);
        else if (lowerArgs.Contains("startup"))
            path = Path.GetDirectoryName(Environment.ProcessPath) ?? path;
        else if (lowerArgs.Contains("documents"))
            path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        else if (lowerArgs.Contains("recent"))
            path = Environment.GetFolderPath(Environment.SpecialFolder.Recent);
        else if (lowerArgs.Contains("windows"))
            path = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
        else if (lowerArgs.Contains("music"))
            path = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
        else if (lowerArgs.Contains("pictures"))
            path = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
        else if (lowerArgs.Contains("videos"))
            path = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
        else if (lowerArgs.Contains("templates"))
            path = Environment.GetFolderPath(Environment.SpecialFolder.Templates);
        else if (lowerArgs.Contains("profile"))
            path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        else if (lowerArgs.Contains("programs"))
            path = Environment.GetFolderPath(Environment.SpecialFolder.Programs);
        else if (lowerArgs.Contains("temp"))
            path = Path.GetTempPath();
        else if (lowerArgs.Contains("root"))
            path = FindProjectRoot();
        else if (lowerArgs.Contains("modules"))
        {
            Environment.CurrentDirectory = AppContext.BaseDirectory;
            Environment.CurrentDirectory = "..\\..\\..\\..\\PainKiller.CommandPrompt.CoreLib\\modules";
            path = Environment.CurrentDirectory;
        }
        path = path.Trim();
        if (Directory.Exists(Environment.CurrentDirectory))
        {
            Environment.CurrentDirectory = Path.GetFullPath(path);
            EventBusService.Service.Publish(new WorkingDirectoryChangedEventArgs(Environment.CurrentDirectory));
        }
        else
        {
            AnsiConsole.MarkupLine($"[red][/]: Path not found: {Markup.Escape(path)}");
            return Nok($"Path not found: {path}");
        }
        if (lowerArgs.Contains("no-output")) return Ok("no output");
        ShowCurrentDirectoryContent();
        return Ok();
    }
    private void UpdateSuggestions(string newWorkingDirectory)
    {
        if (Directory.Exists(newWorkingDirectory))
        {
            var directories = Directory.GetDirectories(newWorkingDirectory)
                .Select(d => new DirectoryInfo(d).Name)
                .ToArray();
            SuggestionProviderManager.AppendContextBoundSuggestions(Identifier, directories.Select(e => e).ToArray());
        }
    }
    private void ShowCurrentDirectoryContent()
    {
        var dirInfo = new DirectoryInfo(Environment.CurrentDirectory);
        var entries = new List<DirEntry>();

        foreach (var dir in dirInfo.GetDirectories())
        {
            var size = dir.GetDirectorySize();
            entries.Add(new DirEntry
            {
                Name = dir.Name,
                Type = "<DIR>",
                SizeInBytes = size,
                Size = size.GetDisplayFormattedFileSize(),
                Updated = dir.LastWriteTime.GetDisplayTimeSinceLastUpdate(),
                UpdatedTime = dir.LastWriteTime
            });
        }
        
        var list = entries.ToList();
        if (list.Count == 0)
        {
            AnsiConsole.MarkupLine("[bold yellow]No directories so show...[/]");
            return;
        }
        var totalSize = list.Sum(e => e.SizeInBytes);
        var formattedSize = totalSize.GetDisplayFormattedFileSize();
        var fileCount = list.Count(e => e.Type != "<DIR>");
        var dirCount = list.Count(e => e.Type == "<DIR>");

        var table = new Table()
            .Expand()
            .RoundedBorder()
            .AddColumn(new TableColumn("[grey]Name[/]").LeftAligned())
            .AddColumn(new TableColumn("[grey]Type[/]").Centered())
            .AddColumn(new TableColumn("[grey]Size[/]").RightAligned())
            .AddColumn(new TableColumn("[grey]Updated[/]").RightAligned());

        foreach (var entry in list)
        {
            var color = entry.Type == "<DIR>" ? "darkmagenta" : "white";
            table.AddRow(
                new Markup($"[{color}]{Markup.Escape(entry.Name)}[/]"),
                new Markup($"[blue]{Markup.Escape(entry.Type)}[/]"),
                new Markup(Markup.Escape(entry.Size)),
                new Markup(Markup.Escape(entry.Updated))
            );
        }

        table.AddEmptyRow();
        table.AddRow(
            new Markup("[bold]Total[/]"),
            new Markup($"{dirCount} folders / {fileCount} files"),
            new Markup($"[bold]{formattedSize}[/]"),
            new Markup($"{list.Count} entries")
        );
        AnsiConsole.Write(table);
    }
    public static string FindProjectRoot()
    {
        var currentDirectory = new DirectoryInfo(AppContext.BaseDirectory);

        while (currentDirectory != null)
        {
            // Först: leta efter en .sln-fil
            if (currentDirectory.GetFiles("*.sln").Any())
            {
                Console.WriteLine($"Solution root found: {currentDirectory.FullName}");
                return currentDirectory.FullName;
            }

            // Därefter: leta efter en .git-katalog
            if (currentDirectory.GetDirectories(".git").Any())
            {
                Console.WriteLine($"Git root found: {currentDirectory.FullName}");
                return currentDirectory.FullName;
            }

            // Gå till överordnad katalog
            currentDirectory = currentDirectory.Parent;
        }

        Console.WriteLine("Project root not found. Reached the root of the filesystem.");
        return string.Empty;
    }
}