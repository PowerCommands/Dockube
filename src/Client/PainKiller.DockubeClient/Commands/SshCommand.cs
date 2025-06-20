using PainKiller.CommandPrompt.CoreLib.Modules.SecurityModule.Extensions;
using Renci.SshNet;

namespace PainKiller.DockubeClient.Commands;

[CommandDesign(     description:  "Run SSH commands",
                        options: ["host", "port", "userName", "password"],
                    suggestions: ["r1","nas"],
                       examples: ["//Run SSH command using ssh declared in configuration","ssh","//Hash password with openssl","ssh \"myPassword\" --password"])]
public class SshCommand(string identifier) : ConsoleCommandBase<CommandPromptConfiguration>(identifier)
{
    public override RunResult Run(ICommandLineInput input)
    {
        if (input.HasOption("password"))
        {
            Console.WriteLine($"Hash: {SslService.GetPassword($"{input.Quotes.FirstOrDefault()}")}");
            return Ok("Password hashed successfully. Use this hash in your configuration.");
        }
        var name = this.GetSuggestion(input.Arguments.FirstOrDefault(), "r1");
        var config = Configuration.Dockube.Ssh.FirstOrDefault(s => s.Name == name);
        if (config == null) return Nok($"No configuration with name {name} exists.");
        
        input.TryGetOption(out var userName, config.UserName);
        input.TryGetOption(out var port, config.Port);
        input.TryGetOption(out var host, config.Host);
        var password = Configuration.Core.Modules.Security.DecryptSecret($"dockube_ssh_{name}");

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