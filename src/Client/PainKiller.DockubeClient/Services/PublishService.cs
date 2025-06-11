using PainKiller.CommandPrompt.CoreLib.Core.Services;
using PainKiller.CommandPrompt.CoreLib.Modules.ShellModule.Services;
using PainKiller.DockubeClient.DomainObjects;
using PainKiller.DockubeClient.Extensions;

namespace PainKiller.DockubeClient.Services;
public class PublishService(string basePath) : IPublishService
{
    public void ExecuteRelease(DockubeRelease release)
    {
        EnsureNamespaceExists(release.Namespace);

        foreach (var res in release.Resources)
        {
            foreach (var cmd in res.Before)
                RunCommand(cmd, "Before");

            var command = res.ToCommand(basePath, release.Name, release.Namespace);
            RunCommand(command, "Apply");

            foreach (var cmd in res.After)
                RunCommand(cmd, "After");
        }
    }
    private void EnsureNamespaceExists(string ns)
    {
        var command = $"create namespace {ns} --dry-run=client -o yaml | kubectl apply -f -";
        RunCommand($"kubectl {command}", "EnsureNamespace");
    }
    private void RunCommand(string command, string stage)
    {
        var fireAndForget = command.StartsWith("$ASYNC$ ", StringComparison.OrdinalIgnoreCase);
        var sleep = command.StartsWith("$SLEEP$", StringComparison.OrdinalIgnoreCase);
        if (sleep)
        {
            Console.WriteLine("Give pod ten seconds to start...");
            Thread.Sleep(10000);
            return;
        }
        ConsoleService.Writer.WriteLine($"[{stage}] Executing command: {command}");
        if (fireAndForget)
        {
            Console.WriteLine("Give pod ten seconds to start...");
            Thread.Sleep(10000);
            command = command[8..].Trim();
            ConsoleService.Writer.WriteLine($"[{stage}] Fire and forget command: {command}");
            ShellService.Default.Execute("cmd.exe", $"/c {command}");
            return;
        }
        var result = ShellService.Default.StartInteractiveProcess("cmd.exe", $"/c {command}");
        ConsoleService.Writer.WriteSuccessLine(result);
    }
}