using Microsoft.Extensions.Logging;
using PainKiller.CommandPrompt.CoreLib.Logging.Services;
namespace PainKiller.CommandPrompt.CoreLib.Core.BaseClasses;
public abstract class ConsoleWriterBase
{
    private readonly ILogger<ConsoleWriterBase> _logger = LoggerProvider.CreateLogger<ConsoleWriterBase>();
    protected void Information(string scope, string text) => _logger.LogInformation($"[{scope}] {text}");
    protected void Warning(string scope, string text) => _logger.LogWarning($"[{scope}] {text}");
    protected void Error(string scope, string text) => _logger.LogError($"[{scope}] {text}");
    protected void Fatal(string scope, string text) => _logger.LogCritical($"[{scope}] {text}");
}