using PainKiller.DockubeClient.Extensions;

namespace PainKiller.DockubeClient.Commands;

[CommandDesign(     description: "Manage the Core Dockube cluster", 
                      arguments: ["<Mode>"],
                         quotes: ["<Namespace name>"],
                        options: ["hosts", "pods", "secrets","tls","endpoints","ingress","namespaces"],
                    suggestions: ["init","\"gitlab\"","\"observation\""],
                       examples: ["//View status of your dockube platform","core","//Initialize the core platform","core init"])]
public class CoreCommand(string identifier) : ConsoleCommandBase<CommandPromptConfiguration>(identifier)
{
    public override RunResult Run(ICommandLineInput input)
    {
        if(this.GetSuggestion(input.Arguments.FirstOrDefault(),"") == "init") return Init();
        StatusCoreCluster(input.Options, $"{input.Quotes.FirstOrDefault()}");
        return Ok();
    }
    private RunResult StatusCoreCluster(IDictionary<string,string> options, string nameSpace)
    {
        var namespaces = Configuration.Dockube.GetReleases().Where(r => r.IsCore && !string.IsNullOrWhiteSpace(r.Namespace)).Select(r => r.Namespace).Distinct().ToList();
        Writer.WriteHeadLine("Core Cluster Status");

        if (options.ContainsKey("namespaces") || options.Count == 0)
        {
            Writer.WriteLine("All Namespaces in Cluster");
            RunCommand("kubectl get namespaces", "Namespaces");
        }

        foreach (var ns in namespaces.Where(n => n.Contains(nameSpace)))
        {
            Writer.WriteHeadLine($"Namespace: {ns}");

            if (options.ContainsKey("pods") || options.Count == 0)
            {
                Writer.WriteLine("Pods");
                RunCommand($"kubectl get pods -n {ns}", $"Pods in {ns}");
            }
            if (options.ContainsKey("secrets") || options.Count == 0)
            {
                Writer.WriteLine("Secrets");
                RunCommand($"kubectl get secrets -n {ns}", $"Secrets in {ns}");
            }

            if (options.ContainsKey("tls") || options.Count == 0)
            {
                Writer.WriteLine("TLS Secrets");
                RunCommand($@"kubectl get secrets -n {ns} -o jsonpath=""{{range .items[?(@.type=='kubernetes.io/tls')]}}{{.metadata.name}}{{'\n'}}{{end}}""", $"TLS Secrets in {ns}");
            }

            if (options.ContainsKey("endpoints") || options.Count == 0)
            {
                Writer.WriteLine("Endpoints");
                RunCommand($"kubectl get endpoints -n {ns}", $"Endpoints in {ns}");
            }

            if (options.ContainsKey("ingress") || options.Count == 0)
            {
                Writer.WriteLine("Ingress Resources");
                RunCommand($"kubectl get ingress -n {ns}", $"Ingress in {ns}");

                Writer.WriteLine("Ingress Hosts");
                RunCommand($@"kubectl get ingress -n {ns} -o jsonpath=""{{range .items[*]}}{{.metadata.name}} {{.spec.rules[*].host}}{{'\n'}}{{end}}""", $"Ingress Hosts in {ns}");
            }
        }
        if (options.ContainsKey("hosts") || options.Count == 0) ShowHosts();
        return Ok();
    }
    private RunResult Init()
    {
        Writer.WriteHeadLine("Initializing Core Cluster...");
        var service = new PublishService(Configuration.Dockube.ManifestBasePath, Configuration.Dockube.Ssl.Output, Configuration.Dockube.Ssl.DefaultCa);
        var coreReleases = Configuration.Dockube.GetReleases().Where(r => r.IsCore).ToList();
        foreach (var release in coreReleases)
        {
            Writer.WriteLine($"Publishing release: {release.Name}");
            service.ExecuteRelease(release);
            Writer.WriteLine($"Release {release.Name} published successfully.");
        }
        ShowHosts();
        return Ok();
    }
    private void ShowHosts()
    {
        var endpoints = Configuration.Dockube.GetReleases().Where(r => r.IsCore).SelectMany(r => r.Resources).Select(res => res.Endpoint).Where(e => !string.IsNullOrWhiteSpace(e)).Distinct().ToList();
        Writer.WriteHeadLine("Hosts");
        foreach (var endpoint in endpoints) Writer.WriteLine($"Endpoint: {endpoint}");
    }
}