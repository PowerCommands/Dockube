using System.Text.RegularExpressions;
using PainKiller.CommandPrompt.CoreLib.Core.BaseClasses;
using PainKiller.CommandPrompt.CoreLib.Core.Events;
using PainKiller.CommandPrompt.CoreLib.Core.Extensions;
using PainKiller.CommandPrompt.CoreLib.Metadata.Attributes;
using PainKiller.CommandPrompt.CoreLib.Modules.ShellModule.DomainObjects;
using PainKiller.CommandPrompt.CoreLib.Modules.ShellModule.Services;
using PainKiller.ReadLine.Managers;
using Spectre.Console;

namespace PainKiller.CommandPrompt.CoreLib.Modules.ShellModule.Commands;

[CommandDesign(
    description: "List the content of the current directory or a target directory, optionally open with File Explorer or show drive info.\n" +
                 "Supports interactive filtering by name, type, size or last updated.\n\n" +
                 "Filter expressions:\n" +
                 "- 'log' → files or folders with 'log' in name/type\n" +
                 "- 'size > 100' → larger than 100 MB\n" +
                 "- 'type = image' → jpg, png, gif, etc\n" +
                 "- 'updated < 30d' → modified in last 30 days",
    options: ["browse", "drive-info", "delete"],
    arguments: ["Path to directory (optional)"],
    examples:
    [
        "//List current folder", "dir",
        "//Browse current directory", "dir --browse",
        "//Show drives", "dir --drive-info",
        "//Enter directory and filter", "dir C:\\temp"
    ]
)]
public class DirCommand : ConsoleCommandBase<ApplicationConfiguration>
{
    private void OnWorkingDirectoryChanged(WorkingDirectoryChangedEventArgs e) => UpdateSuggestions(e.NewWorkingDirectory);
    public DirCommand(string identifier) : base(identifier) => EventBusService.Service.Subscribe<WorkingDirectoryChangedEventArgs>(OnWorkingDirectoryChanged);
    public override RunResult Run(ICommandLineInput input)
    {
        if (input.Options.ContainsKey("drive-info")) return ShowDriveInfo();
        
        var path = input.GetFullPath();
        
        if (!Directory.Exists(path)) return Nok($"Directory not found: {path}");
        if (input.Options.ContainsKey("delete")) return Delete(path);
        if (input.Options.ContainsKey("browse")) ShellService.Default.OpenDirectory(path);
        Environment.CurrentDirectory = path;
        EventBusService.Service.Publish(new WorkingDirectoryChangedEventArgs(path));
        var entries = GetDirectoryEntries();
        SuggestionProviderManager.AppendContextBoundSuggestions(Identifier, entries.Select(e => e.Name).ToArray());

        Writer.Clear();

        string currentFilter = "";

        InteractiveFilter<DirEntry>.Run(
            entries,
            (entry, filter) =>
            {
                currentFilter = filter;
                return EntryFilter(entry, filter);
            },
            (filteredEntries, selectedIndex) =>
            {
                DisplayTable(filteredEntries, selectedIndex, currentFilter);
            }, 
            OnSelected
        );
        return Ok();
    }
    private List<DirEntry> GetDirectoryEntries()
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

        foreach (var file in dirInfo.GetFiles())
        {
            entries.Add(new DirEntry
            {
                Name = file.Name,
                Type = file.GetFileTypeDescription(),
                SizeInBytes = file.Length,
                Size = file.Length.GetDisplayFormattedFileSize(),
                Updated = file.LastWriteTime.GetDisplayTimeSinceLastUpdate(),
                UpdatedTime = file.LastWriteTime
            });
        }

        return entries;
    }
    private bool EntryFilter(DirEntry entry, string filter)
    {
        if (string.IsNullOrWhiteSpace(filter)) return true;
        filter = filter.Trim();
        var sizeMatch = Regex.Match(filter, @"^size\s*(>|<|=)\s*(\d+(\.\d+)?)$", RegexOptions.IgnoreCase);
        if (sizeMatch.Success)
        {
            var op = sizeMatch.Groups[1].Value;
            var thresholdMb = double.Parse(sizeMatch.Groups[2].Value);
            var sizeInMb = entry.SizeInBytes / 1048576.0;

            return op switch
            {
                ">" => sizeInMb > thresholdMb,
                "<" => sizeInMb < thresholdMb,
                "=" => Math.Abs(sizeInMb - thresholdMb) < 0.01,
                _ => false
            };
        }
        var typeMatch = Regex.Match(filter, @"^type\s*=\s*(\w+)$", RegexOptions.IgnoreCase);
        if (typeMatch.Success)
        {
            var category = typeMatch.Groups[1].Value.ToLowerInvariant();
            return category switch
            {
                "image" => IsCategory(entry.Type, ["jpeg", "png", "gif", "bmp", "tiff", "svg", "webp"]),
                "video" => IsCategory(entry.Type, ["mp4", "mkv", "avi", "mov", "wmv", "flv", "webm"]),
                "audio" => IsCategory(entry.Type, ["mp3", "wav", "flac", "aac", "ogg", "m4a"]),
                "code"  => IsCategory(entry.Type, ["c#", "python", "javascript", "html", "css", "java", "php", "cpp", "typescript"]),
                _ => entry.Type.ToLowerInvariant().Contains(category)
            };
        }
        var updatedMatch = Regex.Match(filter, @"^updated\s*(>|<|=)\s*(\d+)([dmy])$", RegexOptions.IgnoreCase);
        if (updatedMatch.Success)
        {
            var op = updatedMatch.Groups[1].Value;
            var value = int.Parse(updatedMatch.Groups[2].Value);
            var unit = updatedMatch.Groups[3].Value.ToLower();

            var threshold = unit switch
            {
                "d" => DateTime.Now.AddDays(-value),
                "m" => DateTime.Now.AddMonths(-value),
                "y" => DateTime.Now.AddYears(-value),
                _ => DateTime.MinValue
            };

            return op switch
            {
                ">" => entry.UpdatedTime < threshold,
                "<" => entry.UpdatedTime > threshold,
                "=" => Math.Abs((entry.UpdatedTime - threshold).TotalDays) < 1,
                _ => false
            };
        }
        return entry.Name.Contains(filter, StringComparison.OrdinalIgnoreCase) || entry.Type.Contains(filter, StringComparison.OrdinalIgnoreCase);
    }
    private bool IsCategory(string type, string[] extensions) => extensions.Any(ext => type.Contains(ext, StringComparison.OrdinalIgnoreCase));
    private void DisplayTable(IEnumerable<DirEntry> entries, int selectedIndex, string? activeFilter)
    {
        var list = entries.ToList();
        if (!string.IsNullOrWhiteSpace(activeFilter))
        {
            AnsiConsole.MarkupLine($"[grey]Active filter:[/] [italic]{Markup.Escape(activeFilter)}[/]");
            Writer.WriteLine();
        }
        if (list.Count == 0)
        {
            AnsiConsole.MarkupLine("[bold yellow]No entries found.[/]");
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
        for (int i = 0; i < list.Count; i++)
        {
            var entry = list[i];
            var isSelected = i == selectedIndex;

            var baseColor = entry.Type == "<DIR>" ? "Blue" : "DarkMagenta";
            var prefix = isSelected ? "[bold cyan]>[/] " : "  ";
            var name = isSelected
                ? $"[bold cyan]{Markup.Escape(entry.Name)}[/]"
                : $"[{baseColor}]{Markup.Escape(entry.Name)}[/]";

            var type = $"[blue]{Markup.Escape(entry.Type)}[/]";
            var size = Markup.Escape(entry.Size);
            var updated = Markup.Escape(entry.Updated);

            table.AddRow(
                new Markup(prefix + name),
                new Markup(type),
                new Markup(size),
                new Markup(updated)
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

    private RunResult ShowDriveInfo()
    {
        foreach (var drive in DriveInfo.GetDrives())
        {
            if (!drive.IsReady) continue;

            AnsiConsole.MarkupLine($"[bold]Drive:[/] {drive.Name}");
            AnsiConsole.MarkupLine($"  Type       : {drive.DriveType}");
            AnsiConsole.MarkupLine($"  Label      : {drive.VolumeLabel}");
            AnsiConsole.MarkupLine($"  Format     : {drive.DriveFormat}");
            AnsiConsole.MarkupLine($"  Total size : {drive.TotalSize.GetDisplayFormattedFileSize()}");
            AnsiConsole.MarkupLine($"  Free space : {drive.TotalFreeSpace.GetDisplayFormattedFileSize()}");
            Writer.WriteLine();
        }
        return Ok();
    }
    private RunResult Delete(string dir)
    {
        var directoryInfo = new DirectoryInfo(dir);
        var confirm = DialogService.YesNoDialog($"Do you want to delete directory {directoryInfo.FullName}?");
        if (confirm)
        {
            Directory.Delete(dir, recursive: true);
            Writer.WriteSuccessLine($"Directory [{directoryInfo.FullName}] deleted.");
        }
        return Ok();
    }
    private void OnSelected(DirEntry entry)
    {
        Writer.Clear();
        Writer.WriteTable([entry]);
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
}