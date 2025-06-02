namespace PainKiller.CommandPrompt.CoreLib.Core.Runtime;
public class CommandRuntime : ICommandRuntime
{
    private readonly Dictionary<string, IConsoleCommand> _commands;
    private readonly CommandExecutor _executor;
    private readonly InputParser _inputParser;
    public CommandRuntime(IEnumerable<IConsoleCommand> commands)
    {
        _commands = commands.ToDictionary(c => c.Identifier, StringComparer.OrdinalIgnoreCase);
        _executor = new CommandExecutor();
        _inputParser = new InputParser();
    }
    public RunResult Execute(string input, string defaultCommand)
    {
        var identifier = input.Trim().Split(' ')[0];
        var commandLineInput = _inputParser.Parse(input);
        var command = _commands.GetValueOrDefault(identifier) ?? _commands.GetValueOrDefault(defaultCommand);
        if(command == null) return new RunResult(commandLineInput.Identifier, false, "Command not found");
        if (commandLineInput.Identifier != command.Identifier) commandLineInput = new CommandLineInput(commandLineInput.Raw, command.Identifier, commandLineInput.Arguments, commandLineInput.Quotes, commandLineInput.Options);
        return _executor.Execute(command, commandLineInput);
    }
}