using System.Text;
using PainKiller.CommandPrompt.CoreLib.Modules.SecurityModule.Extensions;
using PainKiller.ReadLine.Managers;
using Renci.SshNet;

namespace PainKiller.DockubeClient.Commands;

[CommandDesign(     description:  "Dockube -  Run SSH commands",
                        options: ["host", "port", "userName", "password","shutdown"],
                       examples: ["//Run SSH command using ssh declared in configuration","ssh","//Hash password with openssl","ssh \"myPassword\" --password"])]
public class SshCommand(string identifier) : ConsoleCommandBase<CommandPromptConfiguration>(identifier)
{
    private SshClient _client = null!;
    private readonly string _identifier = identifier;

    public override void OnInitialized()
    {
        SuggestionProviderManager.AppendContextBoundSuggestions(_identifier, Configuration.Dockube.Ssh.Select(s => s.Name).ToArray());
        base.OnInitialized();
    }
    public override RunResult Run(ICommandLineInput input)
    {
        if (input.HasOption("password"))
        {
            Console.WriteLine($"Hash: {SslService.GetPassword($"{input.Quotes.FirstOrDefault()}")}");
            return Ok("Password hashed successfully. Use this hash in your configuration.");
        }
        var target = input.Arguments.FirstOrDefault() ?? "";
        if (input.HasOption("shutdown")) return Shutdown(target);

        if (string.IsNullOrEmpty(target)) return Nok("No hostname provided.");

        var config = Configuration.Dockube.Ssh.FirstOrDefault(s => s.Name == target);
        if (config == null) return Nok($"No configuration with name {target} exists.");
        
        input.TryGetOption(out var userName, config.UserName);
        input.TryGetOption(out var port, config.Port);
        input.TryGetOption(out var host, config.Host);
        var password = Configuration.Core.Modules.Security.DecryptSecret($"dockube_ssh_{target}");

        _client = new SshClient(host, port, userName, password);
        _client.Connect();
        Writer.WriteSuccessLine($"Connected to {host}:{port} as {userName}.");
        
        
        using var stream = _client.CreateShellStream("xterm", 80, 24, 800, 600, 1024);
        
        Thread.Sleep(500);
        Console.Write(stream.Read());

        string cmd = "";

        while (cmd != "exit" && cmd != "quit")
        {
            Console.Write("SSH> ");
            cmd = Console.ReadLine() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(cmd)) continue;

            try
            {
                stream.WriteLine(cmd);
                Thread.Sleep(300); // vänta på output
                string response = ReadStream(stream);
                Console.WriteLine(response);
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
    static string ReadStream(ShellStream stream)
    {
        var output = new StringBuilder();
        while (stream.DataAvailable)
        {
            output.Append(stream.Read());
            Thread.Sleep(50);
        }
        return output.ToString();
    }
    public RunResult Shutdown(string target)
    {
        
        foreach (var config in Configuration.Dockube.Ssh.Where(s => s.Host.StartsWith("pi0"))) 
        {
            if (config.Name != target && !string.IsNullOrEmpty(target)) continue;
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