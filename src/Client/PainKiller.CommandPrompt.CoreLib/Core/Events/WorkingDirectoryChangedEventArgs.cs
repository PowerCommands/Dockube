namespace PainKiller.CommandPrompt.CoreLib.Core.Events;
public class WorkingDirectoryChangedEventArgs(string newWorkingDirectory) : EventArgs
{
    public string NewWorkingDirectory { get; } = newWorkingDirectory;
}