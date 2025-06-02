using Spectre.Console;
namespace PainKiller.CommandPrompt.CoreLib.Core.Presentation;
public static class InteractiveFilter<T>
{
    public static void Run(IEnumerable<T> items, Func<T, string, bool> filter, Action<IEnumerable<T>, int> display, Action<T>? onSelected = null, string initialFilter = "")
    {
        var list = items.ToList();
        var filterString = initialFilter;
        var filtered = string.IsNullOrWhiteSpace(filterString)
            ? list
            : list.Where(e => filter(e, filterString)).ToList();
        var selectedIndex = 0;

        while (true)
        {
            ConsoleService.Writer.Clear();
            AnsiConsole.MarkupLine("[grey]Use [italic]Up/Down Arrows[/] to navigate, [italic]Enter[/] to select, [italic]Escape[/] to exit, [italic]Backspace[/] to delete filter, [italic]Any character[/] to filter.[/]");
            display(filtered, selectedIndex);

            if (!string.IsNullOrWhiteSpace(filterString))
                AnsiConsole.MarkupLine($"\n[grey]Filter:[/] [italic]{Markup.Escape(filterString)}[/]");

            var key = Console.ReadKey(intercept: true);
            switch (key.Key)
            {
                case ConsoleKey.Enter:
                    if (onSelected != null && filtered.Count > 0)
                    {
                        var selected = filtered.ElementAtOrDefault(selectedIndex);
                        if (selected != null)
                        {
                            onSelected(selected);
                            return;
                        }
                    }
                    break;

                case ConsoleKey.Escape:
                    return;

                case ConsoleKey.UpArrow:
                    if (selectedIndex > 0) selectedIndex--;
                    break;

                case ConsoleKey.DownArrow:
                    if (selectedIndex < filtered.Count - 1) selectedIndex++;
                    break;

                case ConsoleKey.Backspace:
                    if (filterString.Length > 0)
                    {
                        filterString = filterString[..^1];
                        filtered = list.Where(e => filter(e, filterString)).ToList();
                        selectedIndex = 0;
                    }
                    break;

                default:
                    if (!char.IsControl(key.KeyChar))
                    {
                        filterString += key.KeyChar;
                        filtered = list.Where(e => filter(e, filterString)).ToList();
                        selectedIndex = 0;
                    }
                    break;
            }

            if (selectedIndex >= filtered.Count)
                selectedIndex = Math.Max(0, filtered.Count - 1);
        }
    }
}