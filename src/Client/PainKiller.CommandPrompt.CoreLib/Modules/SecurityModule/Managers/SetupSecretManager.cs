using PainKiller.CommandPrompt.CoreLib.Configuration.Services;
using PainKiller.CommandPrompt.CoreLib.Modules.SecurityModule.Configuration;

namespace PainKiller.CommandPrompt.CoreLib.Modules.SecurityModule.Managers;
public class SetupSecretManager(ApplicationConfiguration applicationConfiguration)
{
    public void InitSecret()
    {
        try
        {
            var setupSecret = DialogService.YesNoDialog("Do you want to setup the encryption keys?");
            if (!setupSecret) return;

            var sharedSecret = AESEncryptionManager.GetStrongRandomString();
            var salt = AESEncryptionManager.GetStrongRandomString(desiredByteLength: 16);

            var environmentKeyNameDefaultName = $"{nameof(CommandPrompt)}_encryptionManager";
            Environment.SetEnvironmentVariable(environmentKeyNameDefaultName, sharedSecret, EnvironmentVariableTarget.User);
            var encryptionConfiguration = new EncryptionConfiguration { SharedSecretEnvironmentKey = environmentKeyNameDefaultName, SharedSecretSalt = salt };
            var fileName = Path.Combine(ApplicationConfiguration.CoreApplicationDataPath, EncryptionConfiguration.SecurityFileName);
            ConfigurationService.Service.Create(encryptionConfiguration, fileName);
            ConsoleService.Writer.WriteSuccessLine("Encryption has been successfully setup!");
        }
        catch (Exception ex)
        {
            ConsoleService.Writer.WriteError(ex.Message, nameof(InitSecret));
        }
    }
}