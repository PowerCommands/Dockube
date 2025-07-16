using PainKiller.CommandPrompt.CoreLib.Modules.ShellModule.Contracts;

namespace PainKiller.CommandPrompt.CoreLib.Modules.ShellModule.DomainObjects;
public sealed class TemporaryDirectory : ITemporaryDirectory
{
    public string Path { get; }

    public TemporaryDirectory()
    {
        Path = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            System.IO.Path.GetRandomFileName());

        Directory.CreateDirectory(Path);
    }
    public void Dispose()
    {
        try
        {
            if (Directory.Exists(Path))
                Directory.Delete(Path, recursive: true);
        }
        catch
        {
            // ignored
        }
    }
}