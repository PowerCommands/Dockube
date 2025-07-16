namespace PainKiller.CommandPrompt.CoreLib.Modules.ShellModule.Contracts;

public interface ITemporaryDirectory : IDisposable
{
    string Path { get; }
}