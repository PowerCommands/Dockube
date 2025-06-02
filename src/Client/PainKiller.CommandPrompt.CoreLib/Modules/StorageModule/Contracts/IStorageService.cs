namespace PainKiller.CommandPrompt.CoreLib.Modules.StorageModule.Contracts;

public interface IStorageService<T> where T : new()
{
    string StoreObject(T storeObject, string fileName = "");
    string DeleteObject(string fileName = "");
    T GetObject(string fileName = "");
    string Backup(string fileName = "");
    List<string> GetFiles();
    DirectoryInfo GetRootDirectory();
    List<string> GetBackupFiles();
    DirectoryInfo GetBackupDirectory();
}