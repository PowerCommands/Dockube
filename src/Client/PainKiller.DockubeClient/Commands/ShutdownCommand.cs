using Microsoft.Extensions.Logging;
using PainKiller.CommandPrompt.CoreLib.Logging.Services;
using PainKiller.CommandPrompt.CoreLib.Modules.SecurityModule.Extensions;
using PainKiller.ReadLine.Managers;
using Renci.SshNet;

namespace PainKiller.DockubeClient.Commands;

[CommandDesign(     description:  "Dockube -  Shutdown a specific server or everyone that starts with the name pi0",
                       examples: ["//Shutdown every configured instance with hostname that starts with pi0","ssh --shutdown",
                                  "//Shutdown specific ssh host","shutdown <host-name>"])]
public class ShutdownCommand(string identifier) : ConsoleCommandBase<CommandPromptConfiguration>(identifier)
{
    private readonly ILogger<ShutdownCommand> _logger = LoggerProvider.CreateLogger<ShutdownCommand>();
    private SshClient _client = null!;
    private readonly string _identifier = identifier;

    public override void OnInitialized()
    {
        SuggestionProviderManager.AppendContextBoundSuggestions(_identifier, Configuration.Dockube.Ssh.Select(s => s.Name).ToArray());
        base.OnInitialized();
    }
    public override RunResult Run(ICommandLineInput input)
    {
        var target = input.Arguments.FirstOrDefault() ?? "";
        return Shutdown(target);
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
            Writer.WriteLine($"{config.Host} sudo poweroff...");
            _logger.LogInformation($"Shutdown command sent to {config.Host}:{config.Port} as {config.UserName}.");
            Thread.Sleep(500);
            Console.WriteLine(command.Result);
        }
        return Quit("Shutdown command sent to all SSH servers.");
    }
}