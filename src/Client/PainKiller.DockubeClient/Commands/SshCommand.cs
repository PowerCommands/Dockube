using PainKiller.CommandPrompt.CoreLib.Modules.SecurityModule.Extensions;
using Renci.SshNet;

namespace PainKiller.DockubeClient.Commands;

[CommandDesign(     description:  "Run SSH commands",
                        options: ["host", "port", "userName"],
                       examples: ["//Run SSH command","//ssh"])]
public class SshCommand(string identifier) : ConsoleCommandBase<CommandPromptConfiguration>(identifier)
{
    public override RunResult Run(ICommandLineInput input)
    {
        input.TryGetOption(out var userName, Configuration.Dockube.Ssh.UserName);
        input.TryGetOption(out var port, Configuration.Dockube.Ssh.Port);
        input.TryGetOption(out var host, Configuration.Dockube.Ssh.Host);
        var password = Configuration.Core.Modules.Security.DecryptSecret("dockube_ssh");

        using var client = new SshClient(host, port, userName, password);
        client.Connect();
        Console.WriteLine($"Connected to {host}:{port} as {userName}.");
        var cmd = "";
        while ( cmd != "exit" && cmd != "quit")
        {
            Console.Write("SSH> ");
            cmd = Console.ReadLine() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(cmd)) continue;
            try
            {
                var command = client.RunCommand(cmd);
                Console.WriteLine(command.Result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
        client.Disconnect();
        Console.WriteLine("Disconnected from SSH server.\n");
        return Ok();
    }
}