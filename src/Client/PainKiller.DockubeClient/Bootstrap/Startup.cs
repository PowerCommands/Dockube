using Microsoft.Extensions.Logging;
using PainKiller.CommandPrompt.CoreLib.Configuration.DomainObjects;
using PainKiller.CommandPrompt.CoreLib.Configuration.Services;
using PainKiller.CommandPrompt.CoreLib.Core.Commands;
using PainKiller.CommandPrompt.CoreLib.Core.Events;
using PainKiller.CommandPrompt.CoreLib.Core.Managers;
using PainKiller.CommandPrompt.CoreLib.Core.Runtime;
using PainKiller.CommandPrompt.CoreLib.Core.Services;
using PainKiller.CommandPrompt.CoreLib.Logging.Services;
using PainKiller.CommandPrompt.CoreLib.Modules.InfoPanelModule.DomainObjects;
using PainKiller.CommandPrompt.CoreLib.Modules.InfoPanelModule.Services;
using PainKiller.ReadLine;
using Serilog;
using Serilog.Events;
using Serilog.Extensions.Logging;
namespace PainKiller.DockubeClient.Bootstrap;

public static class Startup
{
    public static CommandLoop Build()
    {
        Environment.CurrentDirectory = AppContext.BaseDirectory;

        var config = ReadConfiguration();
        Console.Title = config.Core.Name;

        var logger = LoggerProvider.CreateLogger<Program>();
        if (config.Core.Prompt.StartsWith("Warning")) logger.LogCritical($"Configuration file {nameof(CommandPromptConfiguration)}.yaml could not be read or serialized..");
        else logger.LogInformation($"Configuration file {nameof(CommandPromptConfiguration)}.yaml read successfully.");

        if(!Directory.Exists(Path.Combine(ApplicationConfiguration.CoreApplicationDataPath, config.Core.RoamingDirectory))) Directory.CreateDirectory(Path.Combine(ApplicationConfiguration.CoreApplicationDataPath, config.Core.RoamingDirectory));

        ShowLogo(config.Core);
        EventBusService.Service.Subscribe<SetupRequiredEventArgs>(args =>
        {
            logger.LogInformation($"Setup required: {args.Description}");
            args.SetupAction?.Invoke();
        });
        var commands = CommandDiscoveryService.DiscoverCommands(config);
        foreach (var consoleCommand in commands) consoleCommand.OnInitialized();
        
        EventBusService.Service.Subscribe<AfterCommandExecutionEvent>(eventData =>
        {
            Console.Title = config.Core.Name;
        });

        Console.WriteLine();
        Console.WriteLine();

        InfoPanelService.Instance.RegisterContent(new DefaultInfoPanel(new DockubeInfoPanelContent(config.Dockube), config.Core.Modules.InfoPanel));

        var suggestions = new List<string>();
        suggestions.AddRange(config.Core.Suggestions);
        suggestions.AddRange(commands.Select(c => c.Identifier).ToArray());
        ReadLineService.InitializeAutoComplete([], suggestions.ToArray());
        
        logger.LogDebug($"Suggestions: {string.Join(',', suggestions)}");
        
        EventBusService.Service.Publish(new WorkingDirectoryChangedEventArgs(Environment.CurrentDirectory));
        logger.LogDebug($"{nameof(EventBusService)} publish: {nameof(WorkingDirectoryChangedEventArgs)} {Environment.CurrentDirectory}");

        logger.LogInformation($"Started {config.Core.Name} version {config.Core.Version}");

        var logManager = new LogManager(config.Log.FilePath, config.Log.FileName);
        var entries = logManager.GetLog().Take(3);
        LogCommand.DisplayTable(entries, 0);

        return new CommandLoop(new CommandRuntime(commands), new ReadLineInputReader(), config.Core);
    }
    private static CommandPromptConfiguration ReadConfiguration()
    {
        var configuration = ConfigurationService.Service.Get<CommandPromptConfiguration>();
        ConfigureLogging(configuration.Configuration.Log);
        return configuration.Configuration;
    }
    private static void ConfigureLogging(LogConfiguration config)
    {
        var parsedLevel = Enum.TryParse<LogEventLevel>(config.RestrictedToMinimumLevel, ignoreCase: true, out var minimumLevel) ? minimumLevel : LogEventLevel.Information;
        var loggerConfig = new LoggerConfiguration()
            .MinimumLevel.Is(parsedLevel)
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentUserName()
            .Enrich.FromLogContext()
            .WriteTo.File(
                path: Path.Combine(config.FilePath, config.FileName),
                rollingInterval: (RollingInterval) Enum.Parse(typeof(RollingInterval), config.RollingIntervall),
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {MachineName}/{EnvironmentUserName} {SourceContext}: {Message:lj}{NewLine}{Exception}"
            );
        var serilogLogger = loggerConfig.CreateLogger();
        var loggerFactory = new SerilogLoggerFactory(serilogLogger);
        LoggerProvider.Configure(loggerFactory);
    }
    internal static void ShowLogo(CoreConfiguration config, int margin = -1)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        if(!config.ShowLogo) return;
        ConsoleService.WriteCenteredText($" Version {config.Version} ", $"{config.Name}", margin, config.LogoColor);
        Console.WriteLine();
    }
}