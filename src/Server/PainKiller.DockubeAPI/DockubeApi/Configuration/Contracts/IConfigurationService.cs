using DockubeApi.Configuration.DomainObjects;

namespace DockubeApi.Configuration.Contracts;
public interface IConfigurationService
{
    YamlContainer<T> Get<T>(string inputFileName = "") where T : new();
    YamlContainer<T> GetFlexible<T>(string inputFileName = "") where T : new();
    string SaveChanges<T>(T configuration, string inputFileName = "") where T : new();
    void Create<T>(T configuration, string fullFileName) where T : new();    
}