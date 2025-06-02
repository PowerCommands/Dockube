using System.Text.Json;
using System.Text;
using PainKiller.CommandPrompt.CoreLib.Configuration.Services;
using PainKiller.CommandPrompt.CoreLib.Core.Extensions;
using PainKiller.CommandPrompt.CoreLib.Modules.StorageModule.Contracts;

namespace PainKiller.CommandPrompt.CoreLib.Modules.StorageModule.Services;
public class StorageService<T> : IStorageService<T> where T : new()
{
    private readonly string _applicationDataPath;
    private readonly string _backupPath;
    private StorageService()
    {
        var configuration = ConfigurationService.Service.GetFlexible<ApplicationConfiguration>(Path.Combine(AppContext.BaseDirectory, "CommandPromptConfiguration.yaml"));
        _applicationDataPath = configuration.Configuration.Core.Modules.Storage.ApplicationDataFolder.GetReplacedPlaceHolderPath();
        _backupPath = configuration.Configuration.Core.Modules.Storage.BackupPath.GetReplacedPlaceHolderPath();
    }
    public static IStorageService<T> Service { get; } = new StorageService<T>();
    public string StoreObject(T storeObject, string fileName = "")
    {
        var fName = string.IsNullOrEmpty(fileName) ? Path.Combine(_applicationDataPath, $"{typeof(T).Name}.data") : fileName;
        var options = new JsonSerializerOptions { WriteIndented = true, IncludeFields = true };
        var jsonString = JsonSerializer.Serialize(storeObject, options);
        File.WriteAllText(fName, jsonString, Encoding.Unicode);
        return fName;
    }
    public string DeleteObject(string fileName = "")
    {
        var fName = string.IsNullOrEmpty(fileName) ? Path.Combine(_applicationDataPath, $"{typeof(T).Name}.data") : fileName;
        File.Delete(fName);
        return fName;
    }
    public T GetObject(string fileName = "")
    {
        var fName = string.IsNullOrEmpty(fileName) ? Path.Combine(_applicationDataPath, $"{typeof(T).Name}.data") : fileName;
        var options = new JsonSerializerOptions { WriteIndented = true, IncludeFields = true };
        if (!File.Exists(fName)) return new T();
        var jsonString = File.ReadAllText(fName);
        return JsonSerializer.Deserialize<T>(jsonString, options) ?? new T();
    }
    public string Backup(string fileName = "")
    {
        var d = DateTime.Now;
        var sourceFilePath = string.IsNullOrEmpty(fileName) ? Path.Combine(_applicationDataPath, $"{typeof(T).Name}.data") : fileName;
        var backupFilePath = Path.Combine(_backupPath, $"{typeof(T).Name}-{d.Year}{d.Month}{d.Day}{d.Hour}{d.Minute}{d.Second}.data");
        var content = File.ReadAllText(sourceFilePath);
        File.WriteAllText(backupFilePath, content);
        return backupFilePath;
    }
    public List<string> GetFiles() => Directory.GetFiles(_applicationDataPath, "*.data").ToList();
    public DirectoryInfo GetRootDirectory() => new DirectoryInfo(_applicationDataPath);
    public List<string> GetBackupFiles() => Directory.GetFiles(_backupPath, "*.data").ToList();
    public DirectoryInfo GetBackupDirectory() => new DirectoryInfo(_backupPath);
}