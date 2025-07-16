using PainKiller.CommandPrompt.CoreLib.Modules.ShellModule.Contracts;
namespace PainKiller.DockubeClient.DomainObjects;
public class LocalDirectory(string path) : ITemporaryDirectory
{
    public virtual void Dispose() { }
    public string Path { get; } = path ?? throw new ArgumentNullException(nameof(path));
}