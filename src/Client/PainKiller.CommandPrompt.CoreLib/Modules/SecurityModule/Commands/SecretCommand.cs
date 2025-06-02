using Microsoft.Extensions.Logging;
using PainKiller.CommandPrompt.CoreLib.Core.BaseClasses;
using PainKiller.CommandPrompt.CoreLib.Core.Events;
using PainKiller.CommandPrompt.CoreLib.Core.Extensions;
using PainKiller.CommandPrompt.CoreLib.Logging.Services;
using PainKiller.CommandPrompt.CoreLib.Metadata.Attributes;
using PainKiller.CommandPrompt.CoreLib.Modules.SecurityModule.Extensions;
using PainKiller.CommandPrompt.CoreLib.Modules.SecurityModule.Managers;
using PainKiller.CommandPrompt.CoreLib.Modules.SecurityModule.Services;

namespace PainKiller.CommandPrompt.CoreLib.Modules.SecurityModule.Commands;

[CommandDesign(description: "Get, creates, removes or view secrets, first you need to configure your encryption key with initialize argument",
        options: ["create", "initialize", "salt"],
        examples: ["//View all declared secrets", "secret", "//Create secret", "secret --create \"mycommand-pass\"", "//Initialize your machine with a new encryption key (stops if this is already done)", "secret --initialize"])]
public class SecretCommand(string identifier) : ConsoleCommandBase<ApplicationConfiguration>(identifier)
{
    private readonly ILogger<SecretCommand> _logger = LoggerProvider.CreateLogger<SecretCommand>();
    public override void OnInitialized()
    {
        if (CheckEncryptConfiguration()) return;
        var setupManager = new SetupSecretManager(Configuration);
        EventBusService.Service.Publish(new SetupRequiredEventArgs("Encryption key needs to be initialized", setupManager.InitSecret));
    }
    public override RunResult Run(ICommandLineInput input)
    {
        if (input.HasOption("initialize")) return Init();
        if (input.HasOption("salt")) return Salt();
        if (input.HasOption("create")) return Create($"{input.Quotes.FirstOrDefault()}");
        return List();
    }
    private bool CheckEncryptConfiguration()
    {
        try
        {
            var encryptedString = EncryptionService.Service.EncryptString("Encryption is setup properly");
            var decryptedString = EncryptionService.Service.DecryptString(encryptedString);
            _logger.LogInformation(decryptedString);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogCritical($"Encryption is not setup properly. {ex.Message}");
        }
        return false;
    }
    private RunResult Salt()
    {
        Console.WriteLine(AESEncryptionManager.GetStrongRandomString());
        return Ok();
    }
    private RunResult Init()
    {
        var setup = new SetupSecretManager(Configuration);
        setup.InitSecret();
        return Ok();
    }
    private RunResult List()
    {
        foreach (var secret in Configuration.Core.Modules.Security.Secrets) ConsoleService.Writer.WriteDescription(secret.Name, $"{string.Join(',', secret.Options.Keys)}");
        return Ok();
    }
    private RunResult Create(string name)
    {
        var secret = DialogService.GetSecret("secret");
        if (string.IsNullOrEmpty(name)) return Nok("No name provided, secret can´t be created");
        Configuration.Core.Modules.Security.EncryptSecret(EnvironmentVariableTarget.User, name, secret);
        return Ok();
    }
}