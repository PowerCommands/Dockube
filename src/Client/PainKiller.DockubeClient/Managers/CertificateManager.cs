using Microsoft.Extensions.Logging;
using PainKiller.CommandPrompt.CoreLib.Logging.Services;

namespace PainKiller.DockubeClient.Managers;

public class CertificateManager(string domain, string certificateBasePath, string ca) : ICertificateManager
{
    private readonly ILogger<CertificateManager> _logger = LoggerProvider.CreateLogger<CertificateManager>();
    public CreateCertificateResponse CreateCertificate(CertificateRequest request)
    {
        var sslService = SslService.Default;
        var requestSuccess = false;
        var cnFullDomain = $"{request.SubjectCn.Split(' ').First()}.{domain}";
        if (!sslService.CertificateExists(cnFullDomain, certificateBasePath))
        {
            if (request.KeyUsage.ToLower().Trim() == "serverauth")
            {
                var response = sslService.CreateRequestForTls(cnFullDomain, certificateBasePath, request.SubjectCn.Split(' '));
                Console.WriteLine($"Created TLS request for {cnFullDomain} with response: {response}");
                _logger.LogInformation($"Created TLS request for {cnFullDomain} with response: {response}");
                requestSuccess = true;
            }
            else if (request.KeyUsage.ToLower().Trim() == "clientauth")
            {
                var response = sslService.CreateRequestForAuth(cnFullDomain, certificateBasePath, [cnFullDomain]);
                Console.WriteLine($"Created Auth request for {cnFullDomain} with response: {response}");
                _logger.LogInformation($"Created Auth request for {cnFullDomain} with response: {response}");
                requestSuccess = true;
            }
            else
            {
                _logger.LogError($"Unknown key usage type: {request.KeyUsage}");
            }
            if (requestSuccess)
            {
                var response = sslService.CreateAndSignCertificate(cnFullDomain, request.ValidDays, certificateBasePath, ca, request.SubjectCn.Split(' '));
                Console.WriteLine($"Created and signed certificate for {cnFullDomain} with response: {response}");
                _logger.LogInformation($"Created and signed certificate for {cnFullDomain} with response: {response}");
            }
            else
            {
                _logger.LogError($"Failed to create certificate request for {cnFullDomain}.");
                throw new InvalidOperationException($"Failed to create certificate request for {cnFullDomain}.");
            }
        }
        else
        {
            Console.WriteLine($"Certificate for {cnFullDomain} already exists, skipping creation.");
            _logger.LogInformation($"Certificate for {cnFullDomain} already exists, skipping creation.");
        }

        if (!sslService.PemFileExists(cnFullDomain, certificateBasePath))
        {
            var response = sslService.ExportFullChainPemFile(cnFullDomain, ca, certificateBasePath);
            Console.WriteLine($"Exported full chain PEM file for {cnFullDomain} with response: {response}");
            _logger.LogInformation($"Exported full chain PEM file for {cnFullDomain} with response: {response}");
        }
        else
        {
            Console.WriteLine($"Full chain PEM file for {cnFullDomain} already exists, skipping export.");
            _logger.LogInformation($"Full chain PEM file for {cnFullDomain} already exists, skipping export.");
        }

        var safeName = request.SubjectCn.Split(' ').First().Replace(".", "-") + "-tls";
        var cn = request.SubjectCn.Split(' ').First();
        cnFullDomain = $"{cn}.{domain}";

        return new CreateCertificateResponse{SafeName = safeName, FullDomain = cnFullDomain};
    }
}