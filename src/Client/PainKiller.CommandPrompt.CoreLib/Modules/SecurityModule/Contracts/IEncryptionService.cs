namespace PainKiller.CommandPrompt.CoreLib.Modules.SecurityModule.Contracts;
public interface IEncryptionService
{
    string EncryptString(string plainText);
    string DecryptString(string plainText);
}