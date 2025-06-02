namespace PainKiller.CommandPrompt.CoreLib.Core.Events;

public class SetupRequiredEventArgs(string description, Action setupAction) : EventArgs
{
    public string Description { get; } = description;
    public Action SetupAction { get; } = setupAction;
}
