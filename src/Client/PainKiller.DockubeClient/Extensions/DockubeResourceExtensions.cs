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
                return $@"kubectl delete -f ""{fullPath}"" -n {namespaceName} --ignore-not-found";

            case "helm":
                resource.Parameters.TryGetValue("name", out var customName);
                var helmName = !string.IsNullOrWhiteSpace(customName) ? customName : resource.Source;
                return $"helm uninstall {helmName} -n {namespaceName}";
            default:
                throw new NotSupportedException($"Unsupported resource type: {resource.Type}");
        }
    }
}