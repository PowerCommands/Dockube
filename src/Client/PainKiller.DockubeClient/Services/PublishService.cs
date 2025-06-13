using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using PainKiller.CommandPrompt.CoreLib.Core.Services;
using PainKiller.CommandPrompt.CoreLib.Logging.Services;
using PainKiller.CommandPrompt.CoreLib.Modules.ShellModule.Services;
using PainKiller.DockubeClient.Extensions;

namespace PainKiller.DockubeClient.Services;
public class PublishService(string basePath, string certificateBasePath, string ca) : IPublishService
{
    private readonly ILogger<PublishService> _logger = LoggerProvider.CreateLogger<PublishService>();
    public void UninstallRelease(DockubeRelease release)
    {
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
            foreach (var res in release.Resources)
            {
                foreach (var certificate in res.Certificates)
                {
                    CreateCertificate(certificate);
                    Thread.Sleep(1000);
                    var safeName = certificate.SubjectCn.Split(' ').First().Replace(".", "-") + "-tls";
                    var cn = certificate.SubjectCn.Split(' ').First();
                    RunCommand($"kubectl create secret tls {safeName} --cert=ssl-output\\certificate\\{cn}.pem --key=ssl-output\\key\\{cn}.key -n {release.Namespace}", "Certificate");
                }
                foreach (var cmd in res.Before)
                    RunCommand(cmd, "Before");

                var command = res.ToCommand(basePath, release.Name, release.Namespace);
                _logger.LogInformation($"RELEASE {release.Name} NAMESPACE {release.Namespace}");
                _logger.LogInformation($"APPLY {command}");
                RunCommand(command, "Apply");

                foreach (var cmd in res.After)
                    RunCommand(cmd, "After");
                foreach (var secret in res.SecretDescriptors)
                {
                    var retries = 400;
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
                if(!string.IsNullOrEmpty(res.Endpoint)) RunCommand($"echo {res.Endpoint}", "Endpoint: ");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"RELEASE {release.Name} NAMESPACE {release.Namespace}");
            throw;
        }
    }
    private void CreateCertificate(CertificateRequest request)
    {
        var sslService = SslService.Default;
        var requestSuccess = false;
        var cn = request.SubjectCn.Split(' ').First();
        if (!sslService.CertificateExists(cn, certificateBasePath))
        {
            if (request.KeyUsage.ToLower().Trim() == "serverauth")
            {
                var response = sslService.CreateRequestForTls(cn, certificateBasePath, request.SubjectCn.Split(' '));
                Console.WriteLine($"Created TLS request for {request.SubjectCn} with response: {response}");
                _logger.LogInformation($"Created TLS request for {request.SubjectCn} with response: {response}");
                requestSuccess = true;
            }
            else if (request.KeyUsage.ToLower().Trim() == "clientauth")
            {
                var response = sslService.CreateRequestForAuth(cn, certificateBasePath, [cn]);
                Console.WriteLine($"Created Auth request for {cn} with response: {response}");
                _logger.LogInformation($"Created Auth request for {cn} with response: {response}");
                requestSuccess = true;
            }
            else
            {
                _logger.LogError($"Unknown key usage type: {request.KeyUsage}");
            }
            if (requestSuccess)
            {
                var response = sslService.CreateAndSignCertificate(cn, request.ValidDays, certificateBasePath, ca, request.SubjectCn.Split(' '));
                Console.WriteLine($"Created and signed certificate for {cn} with response: {response}");
                _logger.LogInformation($"Created and signed certificate for {cn} with response: {response}");
            }
            else
            {
                _logger.LogError($"Failed to create certificate request for {cn}.");
                throw new InvalidOperationException($"Failed to create certificate request for {cn}.");
            }
        }
        else
        {
            Console.WriteLine($"Certificate for {cn} already exists, skipping creation.");
            _logger.LogInformation($"Certificate for {cn} already exists, skipping creation.");
        }

        if (!sslService.PemFileExists(cn, certificateBasePath))
        {
            var response = sslService.ExportFullChainPemFile(cn, ca, certificateBasePath);
            Console.WriteLine($"Exported full chain PEM file for {cn} with response: {response}");
            _logger.LogInformation($"Exported full chain PEM file for {cn} with response: {response}");
        }
        else
        {
            Console.WriteLine($"Full chain PEM file for {cn} already exists, skipping export.");
            _logger.LogInformation($"Full chain PEM file for {cn} already exists, skipping export.");
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