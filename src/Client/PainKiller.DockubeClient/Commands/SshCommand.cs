using System.Diagnostics;
using System.Text;
using PainKiller.CommandPrompt.CoreLib.Modules.SecurityModule.Extensions;
using PainKiller.ReadLine.Managers;
using Renci.SshNet;

namespace PainKiller.DockubeClient.Commands;

[CommandDesign(     description:  "Dockube -  Run SSH commands, shutdown hosts, hash password...",
                        options: ["shutdown", "host", "port", "userName", "password"],
                       examples: ["//Connect to machine declared in configuration (use tab)","ssh <host-name>","//Hash password with openssl","ssh \"myPassword\" --password",
                                  "//Shutdown every configured instance with hostname that starts with pi0","ssh --shutdown",
                                  "//Shutdown specific ssh host","ssh <host-name> --shutdown"])]
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

        if (!TryConnect(host, port, userName, password))
            return Nok($"Failed to connect to {host}:{port}.");

        using var stream = _client.CreateShellStream("xterm", 80, 24, 800, 600, 1024);
        if (!stream.CanRead || !stream.CanWrite)
            return Nok("ShellStream is not ready for communication.");

        Console.WriteLine(ReadStreamUntil(stream, prompt: "$", timeoutMs: 2000));
        var cmd = string.Empty;

        while (cmd != "exit" && cmd != "quit")
        {
            Console.Write("SSH> ");
            cmd = Console.ReadLine() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(cmd)) continue;

            try
            {
                if (cmd.EndsWith("!"))
                {
                    var raw = cmd.TrimEnd('!');
                    var output = _client.RunCommand(raw);
                    Console.WriteLine(output.Result);
                }
                else
                {
                    stream.WriteLine(cmd);
                    var response = ReadStreamUntil(stream, prompt: "$", timeoutMs: 2000);
                    Console.WriteLine(response);
                }
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
    private bool TryConnect(string host, int port, string user, string password)
    {
        try
        {
            _client = new SshClient(host, port, user, password);
            _client.Connect();
            if (!_client.IsConnected) return false;

            Writer.WriteSuccessLine($"Connected to {host}:{port} as {user}.");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Connection error: {ex.Message}");
            return false;
        }
    }
    private static string ReadStreamUntil(ShellStream stream, string prompt, int timeoutMs)
    {
        var output = new StringBuilder();
        var sw = Stopwatch.StartNew();

        while (sw.ElapsedMilliseconds < timeoutMs)
        {
            if (stream.DataAvailable)
                output.Append(stream.Read());
            else
                Thread.Sleep(50);

            if (output.ToString().TrimEnd().EndsWith(prompt))
                break;
        }
        return output.ToString();
    }
    public RunResult Shutdown(string target)
    {
        foreach (var config in Configuration.Dockube.Ssh.Where(s => s.Host.StartsWith("pi0")))
        {
            if (config.Name != target && !string.IsNullOrEmpty(target)) continue;
            var password = Configuration.Core.Modules.Security.DecryptSecret($"dockube_ssh_{config.Name}");

            if (!TryConnect(config.Host, config.Port, config.UserName, password))
            {
                Console.WriteLine($"Skipping {config.Host} – connection failed.");
                continue;
            }

            try
            {
                var result = _client.RunCommand("sudo poweroff");
                Console.WriteLine(result.Result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Shutdown failed on {config.Host}: {ex.Message}");
            }
        }

        return Ok("Shutdown command sent to all SSH servers.");
    }
}