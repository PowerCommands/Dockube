using PainKiller.PowerCommands.Security.Services;

namespace DockubeCommands.Managers;

public static class SecretManager
{
    public static string CreateSecret(PowerCommandsConfiguration configuration, string accessTokenSecret)
    {
        Console.Write("Enter secret: ");
        var password = PasswordPromptService.Service.ReadPassword();
        Console.WriteLine();
        Console.Write("Confirm secret: ");
        var passwordConfirm = PasswordPromptService.Service.ReadPassword();

        if (password != passwordConfirm)
        {
            Console.WriteLine("Passwords do not match");
            return "";
        }
        var secret = new SecretItemConfiguration { Name = configuration.Constants.GitAccessTokenSecret };
        SecretService.Service.SetSecret(configuration.Constants.GitAccessTokenSecret, password, secret.Options, EncryptionService.Service.EncryptString);

        configuration.Secret ??= new();
        configuration.Secret.Secrets ??= new();
        configuration.Secret.Secrets.Add(secret);
        ConfigurationService.Service.SaveChanges(configuration);
        Console.WriteLine("Configuration and environment variable saved (or updated).");
        return password;
    }
    public static bool CheckEncryptConfiguration()
    {
        try
        {
            var encryptedString = EncryptionService.Service.EncryptString("Encryption is setup properly");
            var decryptedString = EncryptionService.Service.DecryptString(encryptedString);
            Console.WriteLine(decryptedString);
        }
        catch
        {
            Console.WriteLine("\nEncryption is not configured properly");
            return false;
        }
        return true;
    }
}