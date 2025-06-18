using DockubeApi.Configuration.Extensions;
using DockubeApi.Configuration.Contracts;
using DockubeApi.Configuration.DomainObjects;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
namespace DockubeApi.Configuration.Services;
public class ConfigurationService : IConfigurationService
    {
        private ConfigurationService() { }

        private static readonly Lazy<IConfigurationService> Lazy = new(() => new ConfigurationService());
        public static IConfigurationService Service => Lazy.Value;
        public YamlContainer<T> Get<T>(string inputFileName = "") where T : new()
        {
            var fileName = string.IsNullOrEmpty(inputFileName) ? $"{typeof(T).Name}.yaml".GetSafePathRegardlessHowApplicationStarted() : inputFileName.GetSafePathRegardlessHowApplicationStarted();
            var yamlContent = File.ReadAllText(fileName);

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            try
            {
                return deserializer.Deserialize<YamlContainer<T>>(yamlContent);
            }
            catch (Exception)
            {
                Console.WriteLine($"Could not deserialize the configuration file, default configuration will be loaded instead\nA template configuration file named default_{typeof(T).Name}.yaml will be created in application root.");
                var defaultConfig = new T();
                SaveChanges(defaultConfig, $"default_{typeof(T).Name}.yaml");
                return new YamlContainer<T>();
            }
        }

        public YamlContainer<T> GetFlexible<T>(string inputFileName = "") where T : new()
        {
            var fileName = string.IsNullOrEmpty(inputFileName) ? $"{typeof(T).Name}.yaml".GetSafePathRegardlessHowApplicationStarted() : inputFileName.GetSafePathRegardlessHowApplicationStarted();
            var yamlContent = File.ReadAllText(fileName);

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();
            try
            {
                var config = deserializer.Deserialize<YamlContainer<T>>(yamlContent);
                return config;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Could not deserialize the full configuration. Attempting to load base configuration. Error: {ex.Message}");

                try
                {
                    var baseDeserializer = new DeserializerBuilder()
                        .WithNamingConvention(CamelCaseNamingConvention.Instance)
                        .IgnoreUnmatchedProperties()
                        .Build();
                    var baseConfig = baseDeserializer.Deserialize<T>(yamlContent);
                    return new YamlContainer<T> { Configuration = baseConfig };
                }
                catch
                {
                    Console.WriteLine($"Could not deserialize the configuration file even after fallback, default configuration will be loaded instead.\nA template configuration file named default_{typeof(T).Name}.yaml will be created in application root.");
                    var defaultConfig = new T();
                    SaveChanges(defaultConfig, $"default_{typeof(T).Name}.yaml");
                    return new YamlContainer<T>();
                }
            }
        }

        public string SaveChanges<T>(T configuration, string inputFileName = "") where T : new()
        {
            if (configuration is null) return "";
            var fileName = string.IsNullOrEmpty(inputFileName) ? $"{configuration.GetType().Name}.yaml".GetSafePathRegardlessHowApplicationStarted() : inputFileName.GetSafePathRegardlessHowApplicationStarted();

            var yamlContainer = new YamlContainer<T> { Configuration = configuration, Version = "1.0" };
            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            var yamlData = serializer.Serialize(yamlContainer);
            File.WriteAllText(fileName, yamlData);
            return fileName;
        }

        public void Create<T>(T configuration, string fullFileName) where T : new()
        {
            if (configuration is null) return;
            var yamlContainer = new YamlContainer<T> { Configuration = configuration, Version = "1.0" };
            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            var yamlData = serializer.Serialize(yamlContainer);
            File.WriteAllText(fullFileName, yamlData);
        }        
    }