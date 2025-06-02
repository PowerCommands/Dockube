namespace PainKiller.CommandPrompt.CoreLib.Core.BaseClasses;

public abstract class ConsoleCommandBase<TConfig>(string identifier) : IConsoleCommand
{
    protected IConsoleWriter Writer => ConsoleService.Writer;
    public string Identifier { get; } = identifier;
    public TConfig Configuration { get; private set; } = default!;
    protected virtual void SetConfiguration(TConfig config) => Configuration = config;
    public abstract RunResult Run(ICommandLineInput input);
    public virtual void OnInitialized() { }
    protected RunResult Ok(string message = "") => new RunResult(Identifier, true, message);
    protected RunResult Nok(string message = "") => new RunResult(Identifier, false, message);
}
