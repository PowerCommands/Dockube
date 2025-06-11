using Microsoft.Extensions.Logging;
using PainKiller.CommandPrompt.CoreLib.Core.Services;
using PainKiller.CommandPrompt.CoreLib.Logging.Services;
using PainKiller.CommandPrompt.CoreLib.Modules.ShellModule.Services;
using PainKiller.DockubeClient.DomainObjects;
using PainKiller.DockubeClient.Extensions;

namespace PainKiller.DockubeClient.Services;
public class PublishService(string basePath, string certificateBasePath, string ca) : IPublishService
{
    private readonly ILogger<PublishService> _logger = LoggerProvider.CreateLogger<PublishService>();
    public void ExecuteRelease(DockubeRelease release)
    {
        EnsureNamespaceExists(release.Namespace);
        foreach (var res in release.Resources)
        {
            foreach (var certificate in res.Certificates)
            {
                CreateCertificate(certificate);
                Thread.Sleep(1000);
                var safeName = certificate.SubjectCn.Replace(".", "-") + "-tls";
                RunCommand($"kubectl create secret tls {safeName} --cert=ssl-output\\certificate\\{certificate.SubjectCn}.pem --key=ssl-output\\key\\{certificate.SubjectCn}.key -n {release.Namespace}", "Certificate");
            }
            foreach (var cmd in res.Before)
                RunCommand(cmd, "Before");

            var command = res.ToCommand(basePath, release.Name, release.Namespace);
            RunCommand(command, "Apply");

            foreach (var cmd in res.After)
                RunCommand(cmd, "After");
        }
    }
    private void CreateCertificate(CertificateRequest request)
    {
        var sslService = SslService.Default;
        var requestSuccess = false;
        if (!sslService.CertificateExists(request.SubjectCn, certificateBasePath))
        {
            if (request.KeyUsage.ToLower().Trim() == "serverauth")
            {
                var response = sslService.CreateRequestForTls(request.SubjectCn, certificateBasePath, [request.SubjectCn]);
                Console.WriteLine($"Created TLS request for {request.SubjectCn} with response: {response}");
                _logger.LogInformation($"Created TLS request for {request.SubjectCn} with response: {response}");
                requestSuccess = true;
            }
            else if (request.KeyUsage.ToLower().Trim() == "clientauth")
            {
                var response = sslService.CreateRequestForAuth(request.SubjectCn, certificateBasePath, [request.SubjectCn]);
                Console.WriteLine($"Created Auth request for {request.SubjectCn} with response: {response}");
                _logger.LogInformation($"Created Auth request for {request.SubjectCn} with response: {response}");
                requestSuccess = true;
            }
            else
            {
                _logger.LogError($"Unknown key usage type: {request.KeyUsage}");
            }
            if (requestSuccess)
            {
                var response = sslService.CreateAndSignCertificate(request.SubjectCn, request.ValidDays, certificateBasePath, ca);
                Console.WriteLine($"Created and signed certificate for {request.SubjectCn} with response: {response}");
                _logger.LogInformation($"Created and signed certificate for {request.SubjectCn} with response: {response}");
            }
            else
            {
                _logger.LogError($"Failed to create certificate request for {request.SubjectCn}.");
                throw new InvalidOperationException($"Failed to create certificate request for {request.SubjectCn}.");
            }
        }
        else
        {
            _logger.LogInformation($"Certificate for {request.SubjectCn} already exists, skipping creation.");
        }

        if (!sslService.PemFileExists(request.SubjectCn, certificateBasePath))
        {
            var response = sslService.ExportFullChainPemFile(request.SubjectCn, ca, certificateBasePath);
            Console.WriteLine($"Exported full chain PEM file for {request.SubjectCn} with response: {response}");
            _logger.LogInformation($"Exported full chain PEM file for {request.SubjectCn} with response: {response}");
        }
        else
        {
            Console.WriteLine($"Full chain PEM file for {request.SubjectCn} already exists, skipping export.");
            _logger.LogInformation($"Full chain PEM file for {request.SubjectCn} already exists, skipping export.");
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
}