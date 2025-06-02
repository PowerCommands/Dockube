using Microsoft.Extensions.Logging;
using Serilog.Extensions.Logging;
using Serilog;

namespace PainKiller.CommandPrompt.CoreLib.Logging.Services;

public static class LoggerProvider
{
    private static ILoggerFactory? _factory;

    public static void Configure(ILoggerFactory factory)
    {
        _factory = factory;
    }

    public static ILogger<T> CreateLogger<T>()
    {
        if (_factory is not null)
        {
            return _factory.CreateLogger<T>();
        }

        var defaultLogger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();

        var fallbackFactory = new SerilogLoggerFactory(defaultLogger);
        return fallbackFactory.CreateLogger<T>();
    }
}