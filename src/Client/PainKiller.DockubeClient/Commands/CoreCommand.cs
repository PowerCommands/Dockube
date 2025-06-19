using PainKiller.CommandPrompt.CoreLib.Modules.ShellModule.Services;
using System.Text;
using PainKiller.CommandPrompt.CoreLib.Modules.SecurityModule.Extensions;
using PainKiller.DockubeClient.Extensions;

namespace PainKiller.DockubeClient.Commands;

[CommandDesign(description: "Manage the Core Dockube cluster",
                      arguments: ["<Mode>"],
                         quotes: ["<Namespace name>"],
                        options: ["hosts", "passwords", "pods", "secrets", "tls", "endpoints", "ingress", "namespaces", "pvc"],
                    suggestions: ["init", "\"gitlab\"", "\"observation\""],
                       examples: ["//View status of your dockube platform", "core", "//Initialize the core platform", "core init"])]
public class CoreCommand(string identifier) : ConsoleCommandBase<CommandPromptConfiguration>(identifier)
{
    //TODO: taskkill /F /IM code.exe (need some kind of UtilCommand)
    public override RunResult Run(ICommandLineInput input)
    {
        if (this.GetSuggestion(input.Arguments.FirstOrDefault(), "") == "init") return Init();
        if (input.HasOption("passwords")) return GetPasswords();
        StatusCoreCluster(input.Options, $"{input.Quotes.FirstOrDefault()}");
        return Ok();
    }
    private RunResult StatusCoreCluster(IDictionary<string, string> options, string nameSpace)
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
            if (options.ContainsKey("pods") || options.Count == 0)
            {
                Writer.WriteLine($"Pods in namespace: {ns}");
                RunCommand($"kubectl get pods -n {ns}", $"Pods in {ns}");
            }
            if (options.ContainsKey("pvc") || options.Count == 0)
            {
                Writer.WriteLine($"PVC:s in namespace: {ns}");
                RunCommand($"kubectl get pvc -n {ns}", $"PVC:s in {ns}");
            }
            if (options.ContainsKey("secrets") || options.Count == 0)
            {
                Writer.WriteLine($"Secrets in namespace {ns}");
                RunCommand($"kubectl get secrets -n {ns}", $"Secrets in {ns}");
            }

            if (options.ContainsKey("tls") || options.Count == 0)
            {
                Writer.WriteLine($"TLS Secrets in namespace {ns}");
                RunCommand($@"kubectl get secrets -n {ns} -o jsonpath=""{{range .items[?(@.type=='kubernetes.io/tls')]}}{{.metadata.name}}{{'\n'}}{{end}}""", $"TLS Secrets in {ns}");
            }

            if (options.ContainsKey("endpoints") || options.Count == 0)
            {
                Writer.WriteLine($"Endpoints in namespace {ns}");
                RunCommand($"kubectl get endpoints -n {ns}", $"Endpoints in {ns}");
            }

            if (options.ContainsKey("ingress") || options.Count == 0)
            {
                Writer.WriteLine($"Ingress Resources in namespace {ns}");
                RunCommand($"kubectl get ingress -n {ns}", $"Ingress in {ns}");

                Writer.WriteLine($"Ingress Hosts in namespace {ns}");
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
        var releases = Configuration.Dockube.GetReleases().Where(r => r.IsCore).ToList();
        foreach (var dockubeRelease in releases)
        {
            Console.WriteLine($"{dockubeRelease.Name}");
        }
        var endpoints = Configuration.Dockube.GetReleases().Where(r => r.IsCore).SelectMany(r => r.Resources).Select(res => res.Endpoint).Where(e => !string.IsNullOrWhiteSpace(e)).Distinct().ToList();
        Writer.WriteHeadLine("Hosts");
        foreach (var endpoint in endpoints) Writer.WriteLine($"Endpoint: {endpoint}");
    }

    private RunResult GetPasswords()
    {
        var releases = Configuration.Dockube.GetReleases().Where(r => r.IsCore).ToList();
        foreach (var release in releases)
        {
            var releaseName = release.Name;
            if (string.IsNullOrEmpty(releaseName)) return Ok();
            var resource = release.Resources.FirstOrDefault(r => r.SecretDescriptors.Length > 0);
            if (resource != null)
            {
                var base64Encoded = ShellService.Default.StartInteractiveProcess("kubectl.exe", $"get secret {resource.SecretDescriptors.First().Key} -n {release.Namespace} -o jsonpath='{{.data.password}}'").Trim().Replace("'", "");
                var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(base64Encoded));
                Console.WriteLine(releaseName);
                Console.WriteLine(decoded);
            }

            var tokenCommandResource = release.Resources.FirstOrDefault(r => r.After.Any(a => a.ToLower().Contains("create token")));
            var tokenCommand = tokenCommandResource?.After.FirstOrDefault(a => a.ToLower().Contains("create token"))?.Trim();
            if (tokenCommand != null)
            {
                Console.WriteLine($"Access token for {releaseName} (may be short lived)");
                var result = ShellService.Default.StartInteractiveProcess("cmd.exe", $"/c {tokenCommand}");
                Console.WriteLine(result);
            }
        }
        var dockubeGitLabUserPassword = Configuration.Core.Modules.Security.DecryptSecret("dockube-gitlab");
        Console.WriteLine("Gitlab password:");
        Console.WriteLine(dockubeGitLabUserPassword);
        return Ok();
    }
}