using Spectre.Console;
using System.Runtime.CompilerServices;
using PainKiller.CommandPrompt.CoreLib.Core.BaseClasses;
namespace PainKiller.CommandPrompt.CoreLib.Core.Presentation;
public class SpectreConsoleWriter : ConsoleWriterBase, IConsoleWriter
{
    // ReSharper disable once InconsistentNaming
    private static readonly Lazy<SpectreConsoleWriter> _instance = new(() => new SpectreConsoleWriter());
    public static SpectreConsoleWriter Instance => _instance.Value;
    private SpectreConsoleWriter(){}
    private int _reservedLines;
    public void WriteDescription(string label, string text, string title = "Description", bool writeToLog = true, Color? consoleColor = null, bool noBorder = false, [CallerMemberName] string scope = "")
    {
        EnforceMargin();
        var color = consoleColor ?? Color.Blue;
        if (noBorder)
        {
            Write($"{label} : ", writeToLog, color);
            WriteLine(text, writeToLog, Color.Magenta1);
            return;
        }
        var panel = new Panel(new Markup($"[{color}]{label}[/] : [white]{text}[/]"))
        {
            Border = BoxBorder.Rounded,
            Padding = new Padding(1, 1),
            Header = new PanelHeader($"[gray]{title}[/]", Justify.Center),
            Width = (label.Length + text.Length + 5 > title.Length) ? Math.Min(label.Length + text.Length + 5, Console.BufferWidth - 4) : Math.Min(title.Length + 10, Console.BufferWidth - 4)
        };
        AnsiConsole.Write(panel);
        if (writeToLog) Information($"{label} : {text}", scope);
    }
    public void Write(string text, bool writeLog = true, Color? consoleColor = null, [CallerMemberName] string scope = "")
    {
        EnforceMargin();
        var color = consoleColor ?? Color.Black;
        var escaped = Markup.Escape(text);
        AnsiConsole.Markup($"{ToDefaultColorIfBlack(escaped, color)}");
        if (writeLog) Debug(scope, text);
    }

    public void WriteLine(string text = "", bool writeLog = true, Color? consoleColor = null, [CallerMemberName] string scope = "")
    {
        EnforceMargin();
        var color = consoleColor ?? Color.Black;
        var escaped = Markup.Escape(text);
        AnsiConsole.MarkupLine($"{ToDefaultColorIfBlack(escaped, color)}");
        if (writeLog) Debug(scope, text);
    }

    public void WriteSuccessLine(string text, bool writeLog = true, [CallerMemberName] string scope = "")
    {
        EnforceMargin();
        var escaped = Markup.Escape(text);
        AnsiConsole.MarkupLine($"[green]{escaped}[/]");
        if (writeLog) Debug(scope, text);
    }
    public void WriteWarning(string text, string scope)
    {
        EnforceMargin();
        var escaped = Markup.Escape(text);
        AnsiConsole.MarkupLine($"[yellow]{escaped}[/]");
        Warning(scope, text);
    }
    public void WriteError(string text, string scope)
    {
        EnforceMargin();
        var escaped = Markup.Escape(text);
        AnsiConsole.MarkupLine($"[red]{escaped}[/]");
        Error(scope, text);
    }
    public void WriteCritical(string text, string scope)
    {
        EnforceMargin();
        var escaped = Markup.Escape(text);
        AnsiConsole.MarkupLine($"[bold red]{escaped}[/]");
        Fatal(scope, text);
    }
    public void WriteHeadLine(string text, bool writeLog = true, [CallerMemberName] string scope = "")
    {
        EnforceMargin();
        var escaped = Markup.Escape(text);
        AnsiConsole.MarkupLine($"[bold blue]{escaped}[/]");
        if (writeLog) Information(scope, text);
    }
    public void WriteUrl(string text, bool writeLog = true, [CallerMemberName] string scope = "")
    {
        EnforceMargin();
        var escaped = Markup.Escape(text);
        AnsiConsole.MarkupLine($"[underline blue]{escaped}[/]");
        if (writeLog) Information(scope, text);
    }

    public void WritePrompt(string prompt)
    {
        EnforceMargin();
        var escaped = Markup.Escape(prompt);
        AnsiConsole.Markup($"[bold]{escaped} [/]");
    }
    public void WriteRowWithColor(int top, ConsoleColor foregroundColor, ConsoleColor backgroundColor, string rowContent)
    {
        EnforceMargin();
        int originalLeft = Console.CursorLeft;
        int originalTop = Console.CursorTop;

        ConsoleColor originalForeColor = Console.ForegroundColor;
        ConsoleColor originalBackColor = Console.BackgroundColor;

        Console.SetCursorPosition(0, top);

        int consoleWidth = Console.WindowWidth;

        // Clear the row
        Console.SetCursorPosition(0, top);
        Console.Write(new string(' ', consoleWidth));

        // Set the cursor position back to the start of the row
        Console.SetCursorPosition(0, top);

        // Set the new text color for the row
        Console.ForegroundColor = foregroundColor;
        Console.BackgroundColor = backgroundColor;

        // Write the original row content again with the new text color
        Console.Write(rowContent);

        // Restore the original text and background colors
        Console.ForegroundColor = originalForeColor;
        Console.BackgroundColor = originalBackColor;

        // Restore the original cursor position
        Console.SetCursorPosition(originalLeft, originalTop);
    }
    public void Clear()
    {
        AnsiConsole.Clear();
        EnforceMargin();
    }
    public void WriteSeparator(string separator = "-")
    {
        var width = Console.WindowWidth;
        if (string.IsNullOrEmpty(separator)) separator = "-";
        var repeated = new string(separator[0], width);
        Console.WriteLine(repeated);
    }
    public void WriteTable<T>(IEnumerable<T> items, string[]? columnNames = null, Color? consoleColor = null, Color? borderColor = null, bool expand = true)
    {
        EnforceMargin();
        var color = consoleColor ?? Color.DarkSlateGray1;
        var bColor = borderColor ?? Color.DarkSlateGray3;
        var table = new Table().Border(TableBorder.Rounded).BorderColor(bColor);

        if (expand)
        {
            table.Expand();
        }

        var properties = typeof(T).GetProperties();
        bool isFirstColumn = true;

        if (columnNames != null && columnNames.Length == properties.Length)
        {
            foreach (var columnName in columnNames)
            {
                var column = new TableColumn($"[bold {color}]{columnName}[/]");
                if (isFirstColumn)
                {
                    column.LeftAligned();
                    isFirstColumn = false;
                }
                else
                {
                    column.Centered();
                }
                table.AddColumn(column);
            }
        }
        else
        {
            foreach (var property in properties)
            {
                var column = new TableColumn($"[bold {color}]{property.Name}[/]");
                if (isFirstColumn)
                {
                    column.LeftAligned();
                    isFirstColumn = false;
                }
                else
                {
                    column.Centered();
                }
                table.AddColumn(column);
            }
        }

        foreach (var item in items)
        {
            var row = new List<Markup>();

            foreach (var property in properties)
            {
                row.Add(new Markup(Markup.Escape(property.GetValue(item)?.ToString() ?? string.Empty)));
            }
            table.AddRow(row.ToArray());
        }

        AnsiConsole.Write(table);
    }
    public void ClearRow(int top)
    {
        var originalLeft = Console.CursorLeft;
        var originalTop = Console.CursorTop;

        Console.SetCursorPosition(0, top);
        var blankRow = new string(' ', Console.WindowWidth);

        Console.Write(blankRow);
        Console.SetCursorPosition(originalLeft, originalTop);
    }
    public void SetMargin(int reservedLines) => _reservedLines = reservedLines;
    private void EnforceMargin()
    {
        var cursorTop = Console.GetCursorPosition().Top;
        if (cursorTop < _reservedLines)
        {
            Console.SetCursorPosition(0, _reservedLines);
        }
    }
    private string ToDefaultColorIfBlack(string text, Color color) => color == Color.Black ? text : $"[{color}]{text}[/]";
}