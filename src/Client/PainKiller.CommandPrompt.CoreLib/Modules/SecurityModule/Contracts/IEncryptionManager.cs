namespace PainKiller.CommandPrompt.CoreLib.Modules.SecurityModule.Contracts;
public interface IEncryptionManager
{
    string EncryptString(string plainText, string sharedSecret);
    string DecryptString(string cipherText, string sharedSecret);
}