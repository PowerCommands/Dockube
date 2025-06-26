using PainKiller.CommandPrompt.CoreLib.Core.Extensions;
using PainKiller.CommandPrompt.CoreLib.Modules.ShellModule.Services;

namespace PainKiller.CommandPrompt.CoreLib.Core.BaseClasses;

public class TerminalCommando<TConfig>(string identifier) : IConsoleCommand where TConfig : ApplicationConfiguration, new()
{
    protected IConsoleWriter Writer => ConsoleService.Writer;
    public string Identifier { get; } = identifier;
    public TConfig Configuration { get; private set; } = null!;
    protected virtual void SetConfiguration(TConfig config) => Configuration = config;
    public RunResult Run(ICommandLineInput input)
    {
        Console.Title = $"{Identifier} (ext. terminal session)";
        try
        {
            var argument = input.GetRawStringWithIdentifierRemoved();
            ShellService.Default.RunTerminalUntilUserQuits(Identifier, argument);
            return Ok();
        }
        catch (Exception ex)
        {
            Writer.WriteError($"Error executing command '{Identifier}': {ex.Message}", input.Identifier);
            return Nok(ex.Message);
        }
        finally
        {
            Console.Title = Configuration.Core.Name;
        }
    }
    public virtual void OnInitialized() { }
    protected RunResult Ok(string message = "") => new RunResult(Identifier, true, message);
    protected RunResult Nok(string message = "") => new RunResult(Identifier, false, message);
}