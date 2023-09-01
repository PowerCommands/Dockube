using k8s;

namespace DockubeCommands.Managers;

public static class ArgoCdManager
{
    private static string _token = "";
    private static void ReadLine(string output) => _token = output;
    public static async Task<string> GetServiceAccountToken()
    {
        var config = KubernetesClientConfiguration.BuildDefaultConfig();

        using var client = new Kubernetes(config);

        var podList = await client.ListNamespacedPodAsync("argocd"); // Replace "default" with your namespace

        foreach (var pod in podList.Items)
        {
            if (!pod.Metadata.Name.StartsWith("argocd-server-")) continue;
            ShellService.Service.Execute(programName: "kubectl", arguments: $"-n argocd exec -i {pod.Metadata.Name} -- cat /var/run/secrets/kubernetes.io/serviceaccount/token", workingDirectory: AppContext.BaseDirectory, ReadLine, fileExtension: "", waitForExit: true);
            return _token;
        }
        return "";
    }

    public static string GetPassword()
    {
        ShellService.Service.Execute(programName: "kubectl", arguments: $"-n argocd get secret argocd-initial-admin-secret -o jsonpath=\"{{.data.password}}\"", workingDirectory: AppContext.BaseDirectory, ReadLine, fileExtension: "", waitForExit: true);
        return _token;
        
    }
    public static void CreateArgoCdApplication(string repositoryUrlPlaceholder, string userName, string repoName, string repositoryPathPlaceholder, string gitServerIp, string authToken, string applicationName, string repositoryPath, string argoCdTemplateFileName,  string argoCDNamespace)
    {
        var findAndReplaces = new Dictionary<string, string>
        {
            { repositoryUrlPlaceholder, $"http://{gitServerIp}:3000/{userName}/{repoName}" },
            { repositoryPathPlaceholder, repositoryPath }
        };
        var fileName = Path.Combine(AppContext.BaseDirectory, $"{applicationName}.yaml");
        TemplatesManager.FindReplaceFile(findAndReplaces, argoCdTemplateFileName,fileName);
        KubernetesManager.ApplyYamlFile(argoCDNamespace, fileName);

    }
}