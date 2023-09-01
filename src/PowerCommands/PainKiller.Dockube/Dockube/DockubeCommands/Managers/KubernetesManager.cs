using k8s;

namespace DockubeCommands.Managers;
public static class KubernetesManager
{
    public static async Task<string?> GetIpAddress(string containerNameStartsWith, string namespaceName)
    {
        var config = KubernetesClientConfiguration.BuildDefaultConfig();
        using var client = new Kubernetes(config);
        var podList = await client.ListNamespacedPodAsync(namespaceName);
        return podList.Items.Count == 0 ? "" : podList.Items.FirstOrDefault(p => p.Metadata.Name.StartsWith(containerNameStartsWith))?.Status.PodIP;
    }
    public static async Task<bool> IsKubernetesAvailable()
    {
        try
        {
            var config = KubernetesClientConfiguration.BuildDefaultConfig();
            using var client = new Kubernetes(config);
            var podList = await client.ListNamespacedPodAsync("argocd");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return false;
        }
        return true;
    }
    public static void ApplyYamlFile(string nspace, string fileName)
    {
        Action<string> writer = s => ConsoleService.Service.WriteSuccessLine(nameof(KubernetesManager),$"{fileName} applied OK\n");
        var nmnSpace = nspace;
        if (!string.IsNullOrEmpty(nmnSpace) && !fileName.ToLower().Contains("namespace")) nmnSpace = $"-n {nspace}";
        else nmnSpace = "";
        var fileInfo = new FileInfo(fileName);
        ShellService.Service.Execute("kubectl", $"apply {nmnSpace} -f {fileInfo.FullName}", "", writer, "", waitForExit: true);
    }
}