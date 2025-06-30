using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using PainKiller.CommandPrompt.CoreLib.Core.Services;
using PainKiller.CommandPrompt.CoreLib.Logging.Services;
using PainKiller.CommandPrompt.CoreLib.Modules.ShellModule.Services;
using PainKiller.DockubeClient.Extensions;
using PainKiller.DockubeClient.Managers;

namespace PainKiller.DockubeClient.Services;
public class PublishService(string basePath, string templatesPath, string certificateBasePath, string ca, string domain) : IPublishService
{
    private readonly ILogger<PublishService> _logger = LoggerProvider.CreateLogger<PublishService>();
    public void UninstallRelease(DockubeRelease release)
    {
        release.Resources.Reverse();
        foreach (var res in release.Resources)
        {
            var cmd = res.ToUninstallCommand(basePath: basePath, releaseName: release.Name, namespaceName: release.Namespace);
            RunCommand(cmd, "Uninstall Resource");
            
            foreach (var cert in res.Certificates)
            {
                var cn = cert.SubjectCn.Split(' ').First();
                var safeName = cn.Replace(".", "-") + "-tls";

                RunCommand($"kubectl delete secret {safeName} -n {release.Namespace} --ignore-not-found", "Uninstall Certificate");
            }
        }
    }
    public void ExecuteRelease(DockubeRelease release)
    {
        try
        {
            EnsureNamespaceExists(release.Namespace);

            var dir = new DirectoryInfo(Path.Combine(basePath, release.Name));
            if (!dir.Exists)
            {
                dir.Create();
                _logger.LogInformation($"Created directory {dir.FullName}");
            }
            var templatesDir = new DirectoryInfo(Path.Combine(templatesPath, release.Name));
            foreach (var fileInfo in templatesDir.GetFiles("*.yaml"))
            {
                var outputFile = Path.Combine(dir.FullName, fileInfo.Name);

                var placeholders = new Dictionary<string, string>
                {
                    ["$$DOMAIN_NAME$$"]      = domain,
                    ["$$NODE_NAME$$"]        = release.Node,
                    ["$$REPLICAS$$"]         = $"{release.ResourceProfile.Replicas}",
                    ["$$CPU_REQUEST$$"]      = release.ResourceProfile.CpuRequest,
                    ["$$CPU_LIMIT$$"]        = release.ResourceProfile.CpuLimit,
                    ["$$MEMORY_REQUEST$$"]   = release.ResourceProfile.MemoryRequest,
                    ["$$MEMORY_LIMIT$$"]     = release.ResourceProfile.MemoryLimit
                };

                ReplaceManager.ReplacePlaceholdersInFile(fileInfo.FullName, outputFile, placeholders);
            }

            foreach (var res in release.Resources)
            {
                ReplaceManager.DecryptSecrets(templatesPath, basePath, release.Name, res.Source);    //Replaces <ENCRYPTED_STRING> tags with decrypted values

                var certManager = new CertificateManager(domain, certificateBasePath, ca);
                foreach (var certificate in res.Certificates)
                {
                    var certResponse = certManager.CreateCertificate(certificate);
                    Thread.Sleep(1000);
                    RunCommand($"kubectl create secret tls {certResponse.SafeName} --cert=ssl-output\\certificate\\{certResponse.FullDomain}.pem --key=ssl-output\\key\\{certResponse.FullDomain}.key -n {release.Namespace}", "Certificate");
                }
                foreach (var cmd in res.Before)
                    RunCommand(cmd, "Before");

                var ns = string.IsNullOrEmpty(res.OverrideNamespace) ? release.Namespace : res.OverrideNamespace;
                var command = res.ToCommand(basePath, release.Name, ns);

                if (!string.IsNullOrEmpty(res.Source))
                {
                    _logger.LogInformation($"RELEASE {release.Name} NAMESPACE {release.Namespace}");
                    _logger.LogInformation($"APPLY {command}");
                    RunCommand(command, "Apply");
                }

                foreach (var cmd in res.After)
                    RunCommand(cmd, "After");
                foreach (var secret in res.SecretDescriptors)
                {
                    var retries = release.Retries;
                    var base64Encoded = "";
                    while(retries-- > 0)
                    {
                        Thread.Sleep(2000);
                        base64Encoded = ShellService.Default.StartInteractiveProcess("kubectl.exe", $"get secret {secret.Key} -n {release.Namespace} -o jsonpath='{{.data.password}}'").Trim().Replace("'","");
                        if (IsBase64(base64Encoded)) break;
                        _logger.LogWarning($"Secret {secret.Key} not yet available, retrying... ({retries} retries left)");
                        Console.WriteLine($"Secret {secret.Key} not yet available, retrying... ({retries} retries left)");
                        Console.SetCursorPosition(0, Console.CursorTop - 1);
                    }
                    Console.WriteLine(secret.Name);
                    try
                    {
                        var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(base64Encoded));
                        Console.WriteLine(secret.ShowClearText ? decoded : base64Encoded);
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine(e.Message);
                        Console.WriteLine(base64Encoded);
                    }
                }
                if(!string.IsNullOrEmpty(res.Endpoint)) RunCommand($"echo {res.ToEndpoint(domain)}", "Endpoint: ");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"RELEASE {release.Name} NAMESPACE {release.Namespace}");
            throw;
        }
    }
    private void EnsureNamespaceExists(string ns)
    {
        var command = $"create namespace {ns} --dry-run=client -o yaml | kubectl apply -f -";
        RunCommand($"kubectl {command}", "EnsureNamespace");
    }
    private void RunCommand(string command, string stage)
    {
        var fireAndForget = command.StartsWith("$ASYNC$ ", StringComparison.OrdinalIgnoreCase);
        var sleep = command.StartsWith("$SLEEP$", StringComparison.OrdinalIgnoreCase);
        if (sleep)
        {
            Console.WriteLine("Give pod ten seconds to start...");
            Thread.Sleep(10000);
            return;
        }
        ConsoleService.Writer.WriteLine($"[{stage}] Executing command: {command}");
        if (fireAndForget)
        {
            Console.WriteLine("Give pod ten seconds to start...");
            Thread.Sleep(10000);
            command = command[8..].Trim();
            ConsoleService.Writer.WriteLine($"[{stage}] Fire and forget command: {command}");
            ShellService.Default.Execute("cmd.exe", $"/c {command}");
            return;
        }
        var result = ShellService.Default.StartInteractiveProcess("cmd.exe", $"/c {command}");
        ConsoleService.Writer.WriteSuccessLine(result);
    }

    private static readonly Regex Base64Regex = new(@"^[A-Za-z0-9\+/]*={0,2}$", RegexOptions.Compiled);
    public static bool IsBase64(string s)
    {
        if (string.IsNullOrWhiteSpace(s) || s.Length % 4 != 0) 
            return false;
        return Base64Regex.IsMatch(s);
    }
}