using k8s;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;

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
    public static async Task<bool> CreateArgoCdApplication(string uriToArgoCd, string authToken, string applicationName, string repoUrl, string path, Action<string> writeFunction)
    {
        var httpClientHandler = new HttpClientHandler { ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true };
        using var client = new HttpClient(httpClientHandler);
        // Set the ArgoCD API server URL
        var apiServerUrl = $"{uriToArgoCd}/api/v1/applications";
        // Create the application payload using anonymous type
        var payload = new { metadata = new { name = applicationName }, spec = new { source = new { repoURL = repoUrl, path = path, targetRevision = "HEAD" }, destination = new { server = "https://kubernetes.default.svc", }, syncPolicy = new { automated = new { prune = true, selfHeal = true }, syncOptions = Array.Empty<object>() } } };
        // Serialize the payload to JSON
        var jsonPayload = JsonSerializer.Serialize(payload);
        // Convert the JSON payload to a StringContent
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
        // Set the Authorization header
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
        // Make the API request
        var response = await client.PostAsync(apiServerUrl, content);
        // Check the response status
        writeFunction(response.IsSuccessStatusCode ? $"ArgoCD Application {applicationName} created successfully with {apiServerUrl}." : $"Failed to create application. Status code: {response.StatusCode}");
        return response.IsSuccessStatusCode;
    }
}