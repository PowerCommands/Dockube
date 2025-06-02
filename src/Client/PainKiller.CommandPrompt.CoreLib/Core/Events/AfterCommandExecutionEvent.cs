namespace PainKiller.CommandPrompt.CoreLib.Core.Events;
public class AfterCommandExecutionEvent(string commandIdentifier, RunResult result)
{
    public string CommandIdentifier { get; set; } = commandIdentifier;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public RunResult Result { get; set; } = result;
}
