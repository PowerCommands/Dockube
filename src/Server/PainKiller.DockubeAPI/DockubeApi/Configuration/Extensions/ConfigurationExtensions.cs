using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace DockubeApi.Configuration.Extensions;

public static class ConfigurationExtensions
{
    public static string GetSafePathRegardlessHowApplicationStarted(this string fileName, string directory = "") => string.IsNullOrEmpty(directory) ? Path.Combine(AppContext.BaseDirectory, fileName) : Path.Combine(AppContext.BaseDirectory, directory, fileName);
    public static string GetYaml<T>(this T configuration) where T : new()
    {
        var serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        return serializer.Serialize(configuration);
    }
    public static T GetObjectFromYaml<T>(this string yaml) where T : new()
    {
        var serializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        return serializer.Deserialize<T>(yaml);
    }
}