namespace PainKiller.CommandPrompt.CoreLib.Core.Events;
public class CommandsDiscoveredEventArgs(string[] identifiers) : EventArgs
{
    public string[] Identifiers { get; } = identifiers;
}