using Microsoft.Extensions.Logging;
using Serilog.Events;

namespace PainKiller.CommandPrompt.CoreLib.Logging.Extensions;
public static class LoggerExtensions
{
    public static LogEventLevel ToLogLevel(this LogLevel logLevel)
    {
        return logLevel switch
        {
            LogLevel.Trace or LogLevel.Debug => LogEventLevel.Debug,
            LogLevel.Information => LogEventLevel.Information,
            LogLevel.Warning => LogEventLevel.Warning,
            LogLevel.Error => LogEventLevel.Error,
            LogLevel.Critical => LogEventLevel.Fatal,
            LogLevel.None => LogEventLevel.Verbose,
            _ => throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null)
        };
    }
}