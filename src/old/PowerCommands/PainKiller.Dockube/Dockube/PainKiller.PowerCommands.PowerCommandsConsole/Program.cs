using PainKiller.PowerCommands.Core.Services;
using System.Reflection;

ConsoleService.Service.WriteLine(nameof(Program), 

@" ____  _____  ___  _  _  __  __  ____  ____     ___  __    ____    __    ___  
(  _ \(  _  )/ __)( )/ )(  )(  )(  _ \( ___)   / __)(  )  (_  _)  /  )  / _ \ 
 )(_) ))(_)(( (__  )  (  )(__)(  ) _ < )__)   ( (__  )(__  _)(_    )(  ( (_) )
(____/(_____)\___)(_)\_)(______)(____/(____)   \___)(____)(____)  (__)()\___/ ",ConsoleColor.Cyan);
ConsoleService.Service.WriteHeaderLine(nameof(Program),$"\nVersion {ReflectionService.Service.GetVersion(Assembly.GetExecutingAssembly())}");
PainKiller.PowerCommands.Bootstrap.Startup.ConfigureServices().Run(args);