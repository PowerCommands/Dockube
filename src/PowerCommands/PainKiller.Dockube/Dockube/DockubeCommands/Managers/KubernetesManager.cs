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
}