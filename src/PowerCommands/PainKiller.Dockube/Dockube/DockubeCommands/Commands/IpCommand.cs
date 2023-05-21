using k8s;
using PainKiller.PowerCommands.Core.Commands;

namespace DockubeCommands.Commands;

[PowerCommandTest(         tests: " ")]
[PowerCommandDesign( description: "Display ip addresses of you pods",
                         options: "namespace",
                        useAsync: true, 
                         example: "ip")]
public class IpCommand : CdCommand
{
    public IpCommand(string identifier, PowerCommandsConfiguration configuration) : base(identifier, configuration) { }

    public override async Task<RunResult> RunAsync()
    {
        var config = KubernetesClientConfiguration.BuildDefaultConfig();

        using var client = new Kubernetes(config);

        var podList = await client.ListNamespacedPodAsync("argocd"); // Replace "default" with your namespace

        foreach (var pod in podList.Items)
        {
            Console.WriteLine($"Pod Name: {pod.Metadata.Name}");
            Console.WriteLine($"Uid: {pod.Metadata.Uid}");
            Console.WriteLine($"Pod IP: {pod.Status.PodIP}");
            Console.WriteLine();

            if (pod.Metadata.Name.StartsWith("argocd-server-"))
            {
                ShellService.Service.Execute(programName: "kubectl", arguments: $"-n argocd exec -i {pod.Metadata.Name} -- cat /var/run/secrets/kubernetes.io/serviceaccount/token", workingDirectory: WorkingDirectory, ReadLine, fileExtension: "", waitForExit: true);
                WriteLine($"Service account token: {LastReadLine}");
            }
        }
        return Ok();
    }
}