using Microsoft.Extensions.Logging;
using PainKiller.CommandPrompt.CoreLib.Configuration.Services;
using PainKiller.CommandPrompt.CoreLib.Logging.Services;
using PainKiller.CommandPrompt.CoreLib.Modules.SecurityModule.Configuration;
using PainKiller.CommandPrompt.CoreLib.Modules.SecurityModule.Services;

namespace PainKiller.CommandPrompt.CoreLib.Modules.SecurityModule.Extensions;

public static class SecretExtensions
{
    public static string DecryptSecret(this SecurityConfiguration secretConfiguration, string secretName)
    {
        var logger = LoggerProvider.CreateLogger<SecurityConfiguration>();
        try
        {
            var secret = secretConfiguration.Secrets.FirstOrDefault(s => s.Name == secretName);
            if (secret == null) return "";
            var retVal = SecretService.Service.GetSecret(secret.Name, secret.Options, EncryptionService.Service.DecryptString);
            return retVal;
        }
        catch(Exception ex)
        {
            logger.LogError($"{nameof(SecretExtensions)} {nameof(DecryptSecret)} {ex.Message}");
            return "";
        }
    }
    public static string EncryptSecret(this SecurityConfiguration configuration,  EnvironmentVariableTarget target, string secretName, string secret)
    {
        var existing = configuration.Secrets.FirstOrDefault(s => s.Name == secretName);
        if (existing != null) configuration.Secrets.Remove(existing);
        var secretConfiguration = new SecretItemConfiguration(target) { Name = secretName };
        configuration.Secrets.Add(secretConfiguration);
        var retVal = SecretService.Service.SetSecret(secretName, secret, secretConfiguration.Options,EncryptionService.Service.EncryptString);
        ConfigurationService.Service.SaveChanges(configuration);
        return retVal;
    }
    public static string GetBCrypt(this string input)
    {
        return BCrypt.Net.BCrypt.HashPassword(input);
    }
}