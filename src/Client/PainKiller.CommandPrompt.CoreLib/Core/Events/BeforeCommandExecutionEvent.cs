namespace PainKiller.CommandPrompt.CoreLib.Core.Events;
public class BeforeCommandExecutionEvent(string commandIdentifier, ICommandLineInput input)
{
    public string CommandIdentifier { get; set; } = commandIdentifier;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public ICommandLineInput CommandLineInput { get; set; } = input;
}