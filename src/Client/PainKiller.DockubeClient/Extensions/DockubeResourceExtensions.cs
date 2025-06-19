using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace PainKiller.DockubeClient.Extensions;

public static class DockubeResourceExtensions
{
    public static string ToCommand(this DockubeResource resource, string basePath, string releaseName, string namespaceName)
    {
        switch (resource.Type.ToLowerInvariant())
        {
            case "kubectl":
                var fullPath = Path.Combine(basePath, releaseName, resource.Source);
                return $"kubectl apply -f \"{fullPath}\" -n {namespaceName}";
            case "helm":
                resource.Parameters.TryGetValue("version", out var version);
                resource.Parameters.TryGetValue("values", out var valuesFile);

                var chart = resource.Source;
                var valuesPath = string.IsNullOrWhiteSpace(valuesFile) ? "" : $"--values \"{Path.Combine(basePath, releaseName, valuesFile)}\"";

                var versionArg = string.IsNullOrWhiteSpace(version) ? "" : $"--version {version}";
                return $@"helm upgrade --install {chart} {chart}/{chart} -n {namespaceName} {versionArg} {valuesPath}";
            default:
                throw new NotSupportedException($"Unsupported resource type: {resource.Type}");
        }
    }
    public static string ToUninstallCommand(this DockubeResource resource, string basePath, string releaseName, string namespaceName)
    {
        switch (resource.Type.ToLowerInvariant())
        {
            case "kubectl":
                var fullPath = Path.Combine(basePath, releaseName, resource.Source);
                return $@"kubectl delete -f ""{fullPath}"" -n {namespaceName} --ignore-not-found --force --grace-period=0";

            case "helm":
                resource.Parameters.TryGetValue("name", out var customName);
                var helmName = !string.IsNullOrWhiteSpace(customName) ? customName : resource.Source;
                return $"helm uninstall {helmName} -n {namespaceName}";
            default:
                throw new NotSupportedException($"Unsupported resource type: {resource.Type}");
        }
    }

    public static DockubeRelease GetRelease(this DockubeConfiguration configuration, string name)
    {
        var fileName = Path.Combine(configuration.ManifestBasePath, name, "install.yaml");
        var yamlContent = File.ReadAllText(fileName);
        var deserializer = new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
        return deserializer.Deserialize<DockubeRelease>(yamlContent);
    }
    public static List<DockubeRelease> GetReleases(this DockubeConfiguration configuration)
    {
        var releases = new List<DockubeRelease>();
        var manifestDir = new DirectoryInfo(Path.Combine(AppContext.BaseDirectory, configuration.ManifestBasePath));
        foreach (var releaseName in manifestDir.GetDirectories().Select(d => d.Name))
        {
            try
            {
                var release = configuration.GetRelease(releaseName);
                if (release != null) releases.Add(release);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading release '{releaseName}': {ex.Message}");
            }
        }
        return releases;
    }
}