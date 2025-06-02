using System.Text.RegularExpressions;
using PainKiller.CommandPrompt.CoreLib.Core.BaseClasses;
using PainKiller.CommandPrompt.CoreLib.Metadata.Attributes;
using Spectre.Console;

namespace PainKiller.CommandPrompt.CoreLib.Core.Commands;

[CommandDesign("Shows the latest log entries")]
public class LogCommand(string identifier) : ConsoleCommandBase<ApplicationConfiguration>(identifier)
{
    public override RunResult Run(ICommandLineInput input)
    {
        var logDir = Configuration.Log.FilePath;
        if (!Directory.Exists(logDir)) logDir = Path.Combine(AppContext.BaseDirectory, Configuration.Log.FilePath);
        var logFilePrefix = Path.GetFileNameWithoutExtension(Configuration.Log.FileName);

        if (!Directory.Exists(logDir))
            return Nok("Log directory missing.");

        var latestFile = Directory.GetFiles(logDir, $"{logFilePrefix}*.log").OrderByDescending(File.GetLastWriteTime).FirstOrDefault();

        if (latestFile is null)
            return Nok("No log file found.");

        AnsiConsole.MarkupLine($"[bold]Latest log:[/] {Path.GetFileName(latestFile)}");
        var lines = SafeReadLines(latestFile);
        var logEntries = lines
            .Select(ParseLogLine)
            .Select(parsed => new LogEntry { Timestamp = parsed.Timestamp, Level = parsed.Level, Message = parsed.Message }).ToList();
        logEntries.Reverse();
        Writer.Clear();
        bool LogEntryFilter(LogEntry entry, string filter) => entry.Timestamp.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0 || entry.Level.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0 || entry.Message.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0;
        InteractiveFilter<LogEntry>.Run(logEntries, LogEntryFilter, DisplayTable);
        return Ok();
    }
    private (string Timestamp, string Level, string Message) ParseLogLine(string line)
    {
        var match = Regex.Match(line, @"^\[(\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}) (\w+)\]\s+(.+)$");
        if (!match.Success) return ("-", "-", line);
        return (match.Groups[1].Value, match.Groups[2].Value, match.Groups[3].Value);
    }
    private List<string> SafeReadLines(string path)
    {
        var lines = new List<string>();
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream) lines.Add(reader.ReadLine()!);
        return lines;
    }
    private void DisplayTable(IEnumerable<LogEntry> entries, int selectedIndex)
    {
        var list = entries.ToList();
        var table = new Table()
            .RoundedBorder()
            .AddColumn("[grey]Timestamp[/]")
            .AddColumn("[grey]Level[/]")
            .AddColumn("[grey]Message[/]");

        int maxRows = System.Console.WindowHeight - 5; // lite marginal
        int startRow = Math.Max(0, Math.Min(selectedIndex - maxRows / 2, list.Count - maxRows));
        int endRow = Math.Min(list.Count, startRow + maxRows);

        for (int i = startRow; i < endRow; i++)
        {
            var entry = list[i];
            var isSelected = i == selectedIndex;

            var levelColor = entry.Level switch
            {
                "INF" => "green",
                "WRN" => "yellow",
                "ERR" => "red",
                "FTL" => "red",
                "DBG" => "grey",
                _ => "white"
            };

            var prefix = isSelected ? "[bold cyan]>[/] " : "  ";
            var timestamp = isSelected
                ? $"[bold cyan]{Markup.Escape(entry.Timestamp)}[/]"
                : $"[grey]{Markup.Escape(entry.Timestamp)}[/]";

            var level = $"[{levelColor}]{Markup.Escape(entry.Level)}[/]";
            var message = isSelected
                ? $"[italic]{Markup.Escape(entry.Message)}[/]"
                : Markup.Escape(entry.Message);

            table.AddRow(new Markup(prefix + timestamp), new Markup(level), new Markup(message));
        }
        AnsiConsole.Write(table);
    }
    private class LogEntry
    {
        public string Timestamp { get; init; } = "";
        public string Level { get; init; } = "";
        public string Message { get; init; } = "";
    }
}