using Spectre.Console;
namespace PainKiller.CommandPrompt.CoreLib.Core.Services;
public static class ConsoleService
{
    public static readonly IConsoleWriter Writer = SpectreConsoleWriter.Instance;
    public static void WriteCenteredText(string headline, string text, int margin = -1, string color = "DarkMagenta")
    {
        AnsiConsole.Clear();
        if (margin > 0) Console.SetCursorPosition(0, margin);
        var parsedColor = Color.LightGreen;

        if (Enum.TryParse<ConsoleColor>(color, ignoreCase: true, out var consoleColor))
        {
            parsedColor = Color.FromConsoleColor(consoleColor);
        }
        var figlet = new FigletText(text)
            .Centered()
            .Color(parsedColor);

        var panel = new Panel(figlet)
        {
            Border = BoxBorder.Rounded,
            BorderStyle = new Style(foreground: parsedColor),
            Padding = new Padding(1, 1),
            Header = new PanelHeader($"[{color}]{headline}[/]", Justify.Center)
        };
        AnsiConsole.Write(panel);
    }
}