using PainKiller.PowerCommands.Core.Services;

ConsoleService.Service.WriteHeaderLine(nameof(Program), "Dockube Commands 1.0");
PainKiller.PowerCommands.Bootstrap.Startup.ConfigureServices().Run(args);