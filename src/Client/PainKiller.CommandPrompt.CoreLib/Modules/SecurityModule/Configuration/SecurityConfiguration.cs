namespace PainKiller.CommandPrompt.CoreLib.Modules.SecurityModule.Configuration;
public class SecurityConfiguration
{
    public List<SecretItemConfiguration> Secrets { get; set; } = [new SecretItemConfiguration(EnvironmentVariableTarget.User){Name = "Babar"}];
    public EncryptionConfiguration Encryption { get; set; } = new();
}