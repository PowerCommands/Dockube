using System.Text;
using System.Text.RegularExpressions;
using Spectre.Console;

namespace PainKiller.CommandPrompt.CoreLib.Core.Utils;
public static class MarkdownToSpectreConverter
{
    public static string Convert(string markdown)
    {
        var lines = markdown.Split('\n');
        var builder = new StringBuilder();
        var inCodeBlock = false;

        foreach (var rawLine in lines)
        {
            var line = rawLine.TrimEnd();

            if (line.StartsWith("```"))
            {
                inCodeBlock = !inCodeBlock;
                continue;
            }

            if (inCodeBlock)
            {
                builder.AppendLine($"[grey]{Markup.Escape(line)}[/]");
                continue;
            }

            if (line.StartsWith("# "))
                builder.AppendLine($"[bold underline]{Markup.Escape(line[2..])}[/]");
            else if (line.StartsWith("## "))
                builder.AppendLine($"[bold]{Markup.Escape(line[3..])}[/]");
            else if (line.StartsWith("- "))
                builder.AppendLine($"  • {Markup.Escape(line[2..])}");
            else
                builder.AppendLine(ConvertInline(line));
        }

        return builder.ToString();
    }

    private static string ConvertInline(string line)
    {
        line = Markup.Escape(line);
        line = Regex.Replace(line, @"\*\*(.+?)\*\*", "[bold]$1[/]");
        line = Regex.Replace(line, @"\*(.+?)\*", "[italic]$1[/]");
        line = Regex.Replace(line, @"`(.+?)`", "[grey]$1[/]");
        return line;
    }
}