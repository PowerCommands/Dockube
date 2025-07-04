using PainKiller.CommandPrompt.CoreLib.Core.BaseClasses;
using PainKiller.CommandPrompt.CoreLib.Core.Managers;
using PainKiller.CommandPrompt.CoreLib.Metadata.Attributes;
using Spectre.Console;

namespace PainKiller.CommandPrompt.CoreLib.Core.Commands;

[CommandDesign("Shows the latest log entries")]
public class LogCommand(string identifier) : ConsoleCommandBase<ApplicationConfiguration>(identifier)
{
    public override RunResult Run(ICommandLineInput input)
    {
        ILogManager logManager = new LogManager(Configuration.Log.FilePath, Configuration.Log.FileName);
        if (string.IsNullOrEmpty(logManager.CurrentFilePath)) return Nok("No log file found.");

        AnsiConsole.MarkupLine($"[bold]Latest log:[/] {Path.GetFileName(logManager.CurrentFilePath)}");
        var logEntries = logManager.GetLog();
        Writer.Clear();
        bool LogEntryFilter(LogEntry entry, string filter) => entry.Timestamp.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0 || entry.Level.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0 || entry.Message.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0;
        InteractiveFilter<LogEntry>.Run(logEntries, LogEntryFilter, DisplayTable);
        return Ok();
    }
    public static void DisplayTable(IEnumerable<LogEntry> entries, int selectedIndex)
    {
        var list = entries.ToList();
        var table = new Table()
            .RoundedBorder()
            .AddColumn("[grey]Timestamp[/]")
            .AddColumn("[grey]Level[/]")
            .AddColumn("[grey]Message[/]");

        var maxRows = Console.WindowHeight - 5; // lite marginal
        var startRow = Math.Max(0, Math.Min(selectedIndex - maxRows / 2, list.Count - maxRows));
        var endRow = Math.Min(list.Count, startRow + maxRows);

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
            var message = isSelected ? $"[italic]{Markup.Escape(entry.Message)}[/]" : Markup.Escape(entry.Message);
            table.AddRow(new Markup(prefix + timestamp), new Markup(level), new Markup(message));
        }
        AnsiConsole.Write(table);
    }
}