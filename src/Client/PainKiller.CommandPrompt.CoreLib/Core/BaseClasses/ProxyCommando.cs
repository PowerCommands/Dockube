using PainKiller.CommandPrompt.CoreLib.Modules.ShellModule.Services;

namespace PainKiller.CommandPrompt.CoreLib.Core.BaseClasses;

public class ProxyCommando<TConfig>(string identifier, string alias = "") : IConsoleCommand
{
    protected IConsoleWriter Writer => ConsoleService.Writer;
    public string Identifier { get; } = identifier;
    public TConfig Configuration { get; private set; } = default!;
    protected virtual void SetConfiguration(TConfig config) => Configuration = config;
    public RunResult Run(ICommandLineInput input)
    {
        try
        {
            var targetCommand = string.IsNullOrEmpty(alias) ? input.Identifier : alias;
            var argument = input.Raw.Length > input.Identifier.Length ? input.Raw[input.Identifier.Length..].TrimStart() : string.Empty;

            var response = ShellService.Default.StartInteractiveProcess(targetCommand, argument, Environment.CurrentDirectory, waitForExit: true);
            Writer.WriteLine($"{response}");
            return Ok();
        }
        catch(Exception ex)
        {
            Writer.WriteError($"Error executing command '{Identifier}': {ex.Message}", input.Identifier);
            return Nok(ex.Message);
        }
    }
    public virtual void OnInitialized() { }
    protected RunResult Ok(string message = "") => new RunResult(Identifier, true, message);
    protected RunResult Nok(string message = "") => new RunResult(Identifier, false, message);
}