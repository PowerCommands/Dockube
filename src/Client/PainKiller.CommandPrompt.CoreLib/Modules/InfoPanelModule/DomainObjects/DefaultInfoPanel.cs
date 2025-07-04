using Microsoft.Extensions.Logging;
using PainKiller.CommandPrompt.CoreLib.Logging.Services;
using PainKiller.CommandPrompt.CoreLib.Modules.InfoPanelModule.Configuration;
using PainKiller.CommandPrompt.CoreLib.Modules.InfoPanelModule.Contracts;
using Spectre.Console;
namespace PainKiller.CommandPrompt.CoreLib.Modules.InfoPanelModule.DomainObjects;
public class DefaultInfoPanel(IInfoPanelContent content, InfoPanelConfiguration config) : IInfoPanel
{
    private readonly ILogger<DefaultInfoPanel> _logger = LoggerProvider.CreateLogger<DefaultInfoPanel>();
    public void Draw(int margin)
    {
        _ = Task.Run(() =>
        {
            try
            {
                var text = content.GetText();

                var top = Console.CursorTop;
                var left = Console.CursorLeft;
                
                lock (Console.Out)
                {
                    var lines = text.Split(['\n'], StringSplitOptions.None);
                    Clear(margin);

                    for (int i = 0; i < lines.Length && i < margin; i++)
                    {
                        Console.SetCursorPosition(0, i);
                        var padded = lines[i].PadRight(Console.WindowWidth);
                        var bgColor = string.IsNullOrEmpty(config.BackgroundColor) ? "on black" : $"on {config.BackgroundColor}";
                        var fgColor = string.IsNullOrEmpty(config.ForegroundColor) ? "" : $"{config.ForegroundColor} ";
                        var color = (string.IsNullOrEmpty(bgColor) && string.IsNullOrEmpty(fgColor)) ? "" : $"[{fgColor}{bgColor}]";
                        AnsiConsole.MarkupLine($"{color}{padded}[/]");
                    }

                    Console.SetCursorPosition(left, top);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"InfoPanel draw failed: {ex.Message}");
            }
        });
    }

    private void Clear(int margin)
    {
        Console.SetCursorPosition(0, 0);
        var blankLine = new string(' ', Console.WindowWidth);
        var color = string.IsNullOrEmpty(config.BackgroundColor) ? "[on black]" : $"[on {config.BackgroundColor}]";
        for (int i = 0; i < margin; i++)
        {
            AnsiConsole.MarkupLine($"{color}{blankLine}[/]");
        }
    }
}