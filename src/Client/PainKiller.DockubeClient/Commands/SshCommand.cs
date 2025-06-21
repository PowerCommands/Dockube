using PainKiller.CommandPrompt.CoreLib.Modules.SecurityModule.Extensions;
using Renci.SshNet;

namespace PainKiller.DockubeClient.Commands;

[CommandDesign(     description:  "Run SSH commands",
                        options: ["host", "port", "userName", "password", "shutdown"],
                    suggestions: ["r1","nas","shutdown"],
                       examples: ["//Run SSH command using ssh declared in configuration","ssh","//Hash password with openssl","ssh \"myPassword\" --password"])]
public class SshCommand(string identifier) : ConsoleCommandBase<CommandPromptConfiguration>(identifier)
{
    private SshClient _client = null!;
    public override RunResult Run(ICommandLineInput input)
    {
        if (input.HasOption("password"))
        {
            Console.WriteLine($"Hash: {SslService.GetPassword($"{input.Quotes.FirstOrDefault()}")}");
            return Ok("Password hashed successfully. Use this hash in your configuration.");
        }
        var target = this.GetSuggestion(input.Arguments.FirstOrDefault(), "r1");
        
        if (target == "shutdown") return Shutdown();
        
        var config = Configuration.Dockube.Ssh.FirstOrDefault(s => s.Name == target);
        if (config == null) return Nok($"No configuration with name {target} exists.");
        
        input.TryGetOption(out var userName, config.UserName);
        input.TryGetOption(out var port, config.Port);
        input.TryGetOption(out var host, config.Host);
        var password = Configuration.Core.Modules.Security.DecryptSecret($"dockube_ssh_{target}");

        _client = new SshClient(host, port, userName, password);
        _client.Connect();
        Writer.WriteSuccessLine($"Connected to {host}:{port} as {userName}.");
        var cmd = "";
        while ( cmd != "exit" && cmd != "quit")
        {
            Console.Write("SSH> ");
            cmd = Console.ReadLine() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(cmd)) continue;
            try
            {
                var command = _client.RunCommand(cmd);
                Console.WriteLine(command.Result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
        _client.Disconnect();
        Console.WriteLine("Disconnected from SSH server.\n");
        return Ok();
    }
    public RunResult Shutdown()
    {
        foreach (var config in Configuration.Dockube.Ssh.Where(s => s.Host.StartsWith("pi0"))) 
        {
            var password = Configuration.Core.Modules.Security.DecryptSecret($"dockube_ssh_{config.Name}");
            _client = new SshClient(config.Host, config.Port, config.UserName, password);
            _client.Connect();
            Writer.WriteSuccessLine($"Connected to {config.Host}:{config.Port} as {config.UserName}.");
            var command = _client.RunCommand("sudo poweroff");
            Console.WriteLine(command.Result);
        }
        return Ok("Shutdown command sent to all SSH servers.");
    }
}