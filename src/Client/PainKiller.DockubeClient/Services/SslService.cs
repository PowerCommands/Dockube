﻿using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using PainKiller.CommandPrompt.CoreLib.Logging.Services;
using PainKiller.CommandPrompt.CoreLib.Modules.ShellModule.Services;

namespace PainKiller.DockubeClient.Services;
public class SslService : ISslService
{
    private static readonly Lazy<SslService> Instance = new(() => new SslService());    
    public static SslService Default => Instance.Value;
    private SslService() { }

    private readonly ILogger<SslService> _logger = LoggerProvider.CreateLogger<SslService>();
    private const string ExecutableName = "openssl";
    public string GetVersion()
    {
        var version = ShellService.Default.StartInteractiveProcess(ExecutableName, "version", Environment.CurrentDirectory);
        if (!string.IsNullOrEmpty(version)) return version;
        _logger.LogError("OpenSSL executable not found or version command failed.");
        return "OpenSSL executable not found or version command failed.";
    }
    public string CreateRootCertificate(string name, int validDays, string outputFolder)
    {
        if (Directory.Exists(outputFolder) == false) Directory.CreateDirectory(outputFolder);
        var rootDirectory = Path.Combine(outputFolder, "root");
        if (Directory.Exists(rootDirectory) == false) Directory.CreateDirectory(rootDirectory);
        var keyPath = Path.Combine(rootDirectory, "root.key");
        var crtPath = Path.Combine(rootDirectory, "root.crt");

        var arguments = $"req -x509 -new -nodes -keyout \"{keyPath}\" -out \"{crtPath}\" -subj \"/CN={name}\" -days {validDays} -newkey rsa:4096";
        var result = ShellService.Default.StartInteractiveProcess(ExecutableName, arguments, Environment.CurrentDirectory);
        _logger.LogInformation(result);
        return $"Creating root certificate: {name} for {validDays} days at {crtPath}";
    }
    public string CreateIntermediateCertificate(string name, int validDays, string outputFolder)
    {
        var intermediatesDir = Path.Combine(outputFolder, "intermediate", name);
        Directory.CreateDirectory(intermediatesDir);

        var keyPath = Path.Combine(intermediatesDir, "intermediate.key");
        var csrPath = Path.Combine(intermediatesDir, "intermediate.csr");
        var crtPath = Path.Combine(intermediatesDir, "intermediate.crt");
        var serialPath = Path.Combine(intermediatesDir, "intermediate.srl");
        var configPath = Path.Combine(AppContext.BaseDirectory, "Configuration", "intermediate.cnf");

        var rootDir = Path.Combine(outputFolder, "root");
        var rootKey = Path.Combine(rootDir, "root.key");
        var rootCrt = Path.Combine(rootDir, "root.crt");

        if (!File.Exists(rootKey) || !File.Exists(rootCrt))
        {
            _logger.LogError("Root CA not found. Cannot sign intermediate.");
            return "Root certificate or key not found. Please create a root CA first.";
        }
        var csrCmd = $"req -new -newkey rsa:4096 -nodes -keyout \"{keyPath}\" -out \"{csrPath}\" -subj \"/CN={name}\"";
        var csrResult = ShellService.Default.StartInteractiveProcess(ExecutableName, csrCmd, Environment.CurrentDirectory);
        _logger.LogInformation(csrResult);

        if (!File.Exists(csrPath) || !File.Exists(keyPath))
        {
            _logger.LogError("Failed to create intermediate CSR or key.");
            return "Failed to create intermediate CSR or key.";
        }
        var signCmd = $"x509 -req -in \"{csrPath}\" -CA \"{rootCrt}\" -CAkey \"{rootKey}\" -CAcreateserial -CAserial \"{serialPath}\" -out \"{crtPath}\" -days {validDays} -sha256 -extfile \"{configPath}\" -extensions v3_ca";
        var signResult = ShellService.Default.StartInteractiveProcess(ExecutableName, signCmd, Environment.CurrentDirectory);
        _logger.LogInformation(signResult);
        if (!File.Exists(crtPath))
        {
            _logger.LogError("Failed to create signed intermediate certificate.");
            return "Failed to create signed intermediate certificate.";
        }
        return $"✔ Intermediate CA created: {crtPath}\n✔ Key: {keyPath}\n✔ Signed by: {rootCrt}";
    }
    public bool CertificateExists(string commonName, string outputFolder)
    {
        var crtDir = Path.Combine(outputFolder, "certificate");
        var crtPath = Path.Combine(crtDir, $"{commonName}.crt");
        return File.Exists(crtPath);
    }
    public bool PemFileExists(string commonName, string outputFolder)
    {
        var crtDir = Path.Combine(outputFolder, "certificate");
        var crtPath = Path.Combine(crtDir, $"{commonName}.pem");
        return File.Exists(crtPath);
    }
    public string CreateRequestForTls(string commonName, string outputFolder, IEnumerable<string>? sanList = null)
    {
        var keyDir = Path.Combine(outputFolder, "key");
        var reqDir = Path.Combine(outputFolder, "request");

        Directory.CreateDirectory(keyDir);
        Directory.CreateDirectory(reqDir);

        var keyPath = Path.Combine(keyDir, $"{commonName}.key");
        var csrPath = Path.Combine(reqDir, $"{commonName}.csr");

        var eku = "extendedKeyUsage = serverAuth";
        var ku = "keyUsage = digitalSignature,keyEncipherment";

        var sanItems = new List<string> { $"DNS:{commonName}" }; // always include CN
        if (sanList != null)
        {
            foreach (var item in sanList.Where(s => !string.IsNullOrEmpty(s)))
            {
                sanItems.Add(IPAddress.TryParse(item, out _) ? $"IP:{item}" : $"DNS:{item}");
            }
        }
        var san = $"subjectAltName = {string.Join(",", sanItems.Distinct())}";

        var genRequest =
            $"req -new -newkey rsa:2048 -nodes " +
            $"-keyout \"{keyPath}\" -out \"{csrPath}\" " +
            $"-subj \"/CN={commonName}\" " +
            $"-addext \"{eku}\" -addext \"{ku}\" -addext \"{san}\"";

        var result = ShellService.Default.StartInteractiveProcess(ExecutableName, genRequest, Environment.CurrentDirectory);
        _logger.LogInformation(result);

        if (!File.Exists(csrPath) || !File.Exists(keyPath))
        {
            _logger.LogError($"Failed to create CSR or key for '{commonName}'.");
            return $"Failed to create CSR or key for '{commonName}'.";
        }

        return $"✔ TLS Private key: {keyPath}\n✔ TLS CSR: {csrPath}\n✔ SAN: {string.Join(", ", sanItems)}";
    }
    public string CreateRequestForAuth(string commonName, string outputFolder, IEnumerable<string>? sanList = null)
    {
        var keyDir = Path.Combine(outputFolder, "key");
        var reqDir = Path.Combine(outputFolder, "request");

        Directory.CreateDirectory(keyDir);
        Directory.CreateDirectory(reqDir);

        var keyPath = Path.Combine(keyDir, $"{commonName}.key");
        var csrPath = Path.Combine(reqDir, $"{commonName}.csr");

        var eku = "extendedKeyUsage = clientAuth";
        var ku = "keyUsage = digitalSignature";

        var sanItems = new List<string> { commonName.Contains('@') ? $"email:{commonName}" : $"URI:spiffe://dockube/{commonName}" };
        if (sanList != null)
        {
            foreach (var item in sanList)
            {
                if (item.Contains("@"))
                    sanItems.Add($"email:{item}");
                else if (item.Contains("://"))
                    sanItems.Add($"URI:{item}");
                else if (IPAddress.TryParse(item, out _))
                    sanItems.Add($"IP:{item}");
                else
                    sanItems.Add($"DNS:{item}");
            }
        }
        var san = $"subjectAltName = {string.Join(",", sanItems.Distinct())}";
        var genRequest =
            $"req -new -newkey rsa:2048 -nodes " +
            $"-keyout \"{keyPath}\" -out \"{csrPath}\" " +
            $"-subj \"/CN={commonName}\" " +
            $"-addext \"{eku}\" -addext \"{ku}\" -addext \"{san}\"";

        var result = ShellService.Default.StartInteractiveProcess(ExecutableName, genRequest, Environment.CurrentDirectory);
        _logger.LogInformation(result);

        if (!File.Exists(csrPath) || !File.Exists(keyPath))
        {
            _logger.LogError($"Failed to create CSR or key for '{commonName}'.");
            return $"Failed to create CSR or key for '{commonName}'.";
        }

        return $"✔ Auth Private key: {keyPath}\n✔ Auth CSR: {csrPath}\n✔ SAN: {string.Join(", ", sanItems)}";
    }
    public string CreateAndSignCertificate(string commonName, int validDays, string outputFolder, string caName, IEnumerable<string>? sanList = null)
    {
        var reqDir = Path.Combine(outputFolder, "request");
        var crtDir = Path.Combine(outputFolder, "certificate");
        var caDir = Path.Combine(outputFolder, "intermediate", caName);

        Directory.CreateDirectory(crtDir);

        var csrPath = Path.Combine(reqDir, $"{commonName}.csr");
        var crtPath = Path.Combine(crtDir, $"{commonName}.crt");

        var caCrt = Path.Combine(caDir, "intermediate.crt");
        var caKey = Path.Combine(caDir, "intermediate.key");
        var serialPath = Path.Combine(caDir, "intermediate.srl");

        var sanItems = new List<string> { commonName  };
        if (sanList != null)
        {
            foreach (var item in sanList.Where(s => !string.IsNullOrEmpty(s)))
            {
                sanItems.Add(item);
            }
        }
        if (!File.Exists(caCrt) || !File.Exists(caKey))
        {
            _logger.LogError($"Intermediate CA '{caName}' not found in '{caDir}'.");
            return $"Intermediate CA '{caName}' not found. Please create it first.";
        }

        var certTemplateConfig = Path.Combine(AppContext.BaseDirectory, "Configuration", "cert.cnf");
        var certOutputConfig = Path.Combine(AppContext.BaseDirectory, $"cert.{commonName}.cnf");

        GenerateOpenSslConfig(certTemplateConfig, certOutputConfig, commonName, sanItems);

        var signRequest = $"x509 -req " + $"-in \"{csrPath}\" " + $"-CA \"{caCrt}\" " + $"-CAkey \"{caKey}\" " + $"-CAcreateserial -CAserial \"{serialPath}\" " + $"-out \"{crtPath}\" " + $"-days {validDays} " + $"-sha256 " + $"-extfile \"{certOutputConfig}\" -extensions req_ext";

        var result = ShellService.Default.StartInteractiveProcess(ExecutableName, signRequest, Environment.CurrentDirectory);
        _logger.LogInformation(result);

        if (!File.Exists(crtPath))
        {
            _logger.LogError($"Failed to create signed certificate for '{commonName}' with CA '{caName}'.");
            return $"Failed to create signed certificate for '{commonName}' with CA '{caName}'.";
        }

        return $"✔ Certificate created: {crtPath}\n✔ Signed by Intermediate CA: {caName}";
    }
    public CertificateInfo InspectCertificate(string certPath)
    {
        if (!File.Exists(certPath))
        {
            var cwd = Directory.GetCurrentDirectory();
            var fallbackCert = Directory.EnumerateFiles(cwd)
                .FirstOrDefault(f =>
                    f.EndsWith(".crt", StringComparison.OrdinalIgnoreCase) ||
                    f.EndsWith(".cer", StringComparison.OrdinalIgnoreCase));

            if (fallbackCert != null)
            {
                _logger.LogWarning("Specified certificate not found, using fallback: {Fallback}", Path.GetFileName(fallbackCert));
                certPath = fallbackCert;
            }
            else
            {
                throw new FileNotFoundException("Certificate file not found and no fallback .crt or .cer found in working directory.", certPath);
            }
        }
        using var cert = X509CertificateLoader.LoadCertificateFromFile(certPath);
        var info = new CertificateInfo
        {
            FileName = Path.GetFileName(certPath),
            SubjectCn = cert.GetNameInfo(X509NameType.DnsName, false),
            Issuer = cert.Issuer,
            ValidFrom = cert.NotBefore,
            ValidTo = cert.NotAfter,
            IsValidNow = DateTime.Now >= cert.NotBefore && DateTime.Now <= cert.NotAfter,
            ThumbprintSha1 = cert.Thumbprint
        };
        var keyUsage = cert.Extensions.OfType<X509KeyUsageExtension>().FirstOrDefault();
        if (keyUsage != null) info.KeyUsage = keyUsage.KeyUsages.ToString();
        
        var eku = cert.Extensions.OfType<X509EnhancedKeyUsageExtension>().FirstOrDefault();
        if (eku != null) info.ExtendedKeyUsages = eku.EnhancedKeyUsages.Cast<Oid>().Select(o => o.FriendlyName).ToList();
        var sanExt = cert.Extensions.FirstOrDefault(e => e.Oid?.Value == "2.5.29.17");
        if (sanExt != null)
        {
            var sanFormatted = sanExt.Format(true);
            info.SubjectAlternativeNames = sanFormatted.Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();
        }
        return info;
    }
    public string ExportFullChainPemFile(string host, string intermediateDirectory, string output)
    {
        var certPath = Path.Combine(output, "certificate", $"{host}.crt");
        var intermediatePath = Path.Combine(output, "intermediate", intermediateDirectory, "intermediate.crt");

        if (!File.Exists(certPath) || !File.Exists(intermediatePath))
            throw new FileNotFoundException("Required cert files missing.");

        var fullChainPath = Path.Combine(output, "certificate" ,$"{host}.pem");
        File.WriteAllText(fullChainPath, File.ReadAllText(certPath) + "\n" + File.ReadAllText(intermediatePath));
        Thread.Sleep(500);
        ExportFullChainCertFile(host, output);
        return fullChainPath;
    }
    private void ExportFullChainCertFile(string host, string output)
    {
        var pemPath = Path.Combine(output, "certificate", $"{host}.pem");
        var keyPath = Path.Combine(output, "key", $"{host}.key");

        if (!File.Exists(pemPath) || !File.Exists(keyPath))
            throw new FileNotFoundException("Required cert files missing.");

        var certPath = Path.Combine(output, "certificate" ,$"{host}.cert");
        File.WriteAllText(certPath, File.ReadAllText(pemPath) + "\n" + File.ReadAllText(keyPath));
    }
    public string ExportToPfx(string commonName, string intermediateDirectory, string outputFolder, string password = "mypassword")
    {
        var certDir = Path.Combine(outputFolder, "certificate");
        var keyDir = Path.Combine(outputFolder, "key");
        var intermediateDir = Path.Combine(outputFolder, "intermediate", intermediateDirectory);

        var certPath = Path.Combine(certDir, $"{commonName}.crt");
        var keyPath = Path.Combine(keyDir, $"{commonName}.key");
        var intermediatePath = Path.Combine(intermediateDir, "intermediate.crt");
        var pfxPath = Path.Combine(certDir, $"{commonName}.pfx");
        var systemPfxPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".aspnet", "https", $"{commonName}.pfx");
        
        if (!File.Exists(certPath) || !File.Exists(keyPath))
            return $"❌ Missing required files for {commonName}.";

        var command = $"pkcs12 -export " +
                      $"-inkey \"{keyPath}\" " +
                      $"-in \"{certPath}\" " +
                      $"{(File.Exists(intermediatePath) ? $"-certfile \"{intermediatePath}\" " : "")}" +
                      $"-out \"{pfxPath}\" " +
                      $"-passout pass:{password}";

        var result = ShellService.Default.StartInteractiveProcess("openssl", command, Environment.CurrentDirectory);

        if (!File.Exists(pfxPath))
            return $"❌ Failed to export PFX for {commonName}.\n🔧 Output:\n{result}";
        
        File.Copy(pfxPath, systemPfxPath, overwrite: true);

        return $"✔ Exported PFX: {pfxPath} and {pfxPath}\n🔐 Password: {password}";
    }
    private void GenerateOpenSslConfig(string templatePath, string outputPath, string commonName, IEnumerable<string> sanList)
    {
        if (!File.Exists(templatePath)) throw new FileNotFoundException("Template file not found.", templatePath);

        var template = File.ReadAllText(templatePath);
        template = template.Replace("$NAME$", commonName);
        var dnsEntries = new List<string>();
        int index = 1;
        foreach (var entry in sanList.Distinct())
        {
            if (IPAddress.TryParse(entry, out _))
                dnsEntries.Add($"IP.{index} = {entry}");
            else
                dnsEntries.Add($"DNS.{index} = {entry}");

            index++;
        }
        var dnsBlock = string.Join(Environment.NewLine, dnsEntries);
        template = template.Replace("$DNS$", dnsBlock);
        File.WriteAllText(outputPath, template);
    }
    public static string GetPassword(string plaintext)
    {
        var command = $"passwd -6 {plaintext}";
        var result = ShellService.Default.StartInteractiveProcess("openssl", command, Environment.CurrentDirectory);
        return result;
    }
}