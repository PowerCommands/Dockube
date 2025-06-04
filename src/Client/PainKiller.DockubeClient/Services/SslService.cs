using System.Net;
using Microsoft.Extensions.Logging;
using PainKiller.CommandPrompt.CoreLib.Logging.Services;
using PainKiller.CommandPrompt.CoreLib.Modules.ShellModule.Services;

namespace PainKiller.DockubeClient.Services;
public class SslService(string executablePath) : ISslService
{
    private readonly ILogger<SslService> _logger = LoggerProvider.CreateLogger<SslService>();

    private readonly string _fullPath = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), executablePath), "openssl.exe");
    public string GetVersion()
    {
        if (string.IsNullOrEmpty(executablePath)) return "";
        var version = ShellService.Default.StartInteractiveProcess(_fullPath, "version");
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
        var result = ShellService.Default.StartInteractiveProcess(_fullPath, arguments);
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
        var csrResult = ShellService.Default.StartInteractiveProcess(_fullPath, csrCmd);
        _logger.LogInformation(csrResult);

        if (!File.Exists(csrPath) || !File.Exists(keyPath))
        {
            _logger.LogError("Failed to create intermediate CSR or key.");
            return "Failed to create intermediate CSR or key.";
        }
        var signCmd = $"x509 -req -in \"{csrPath}\" -CA \"{rootCrt}\" -CAkey \"{rootKey}\" -CAcreateserial -CAserial \"{serialPath}\" -out \"{crtPath}\" -days {validDays} -sha256 -extfile \"{configPath}\" -extensions v3_ca";
        var signResult = ShellService.Default.StartInteractiveProcess(_fullPath, signCmd);
        _logger.LogInformation(signResult);
        if (!File.Exists(crtPath))
        {
            _logger.LogError("Failed to create signed intermediate certificate.");
            return "Failed to create signed intermediate certificate.";
        }
        return $"✔ Intermediate CA created: {crtPath}\n✔ Key: {keyPath}\n✔ Signed by: {rootCrt}";
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

        var result = ShellService.Default.StartInteractiveProcess(_fullPath, genRequest);
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

        var result = ShellService.Default.StartInteractiveProcess(_fullPath, genRequest);
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

        var sanItems = new List<string> { {commonName} }; // always include CN
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

        var result = ShellService.Default.StartInteractiveProcess(_fullPath, signRequest);
        _logger.LogInformation(result);

        if (!File.Exists(crtPath))
        {
            _logger.LogError($"Failed to create signed certificate for '{commonName}' with CA '{caName}'.");
            return $"Failed to create signed certificate for '{commonName}' with CA '{caName}'.";
        }

        return $"✔ Certificate created: {crtPath}\n✔ Signed by Intermediate CA: {caName}";
    }

    private void GenerateOpenSslConfig(string templatePath, string outputPath, string commonName, IEnumerable<string> sanList)
    {
        if (!File.Exists(templatePath))
        {
            throw new FileNotFoundException("Template file not found.", templatePath);
        }

        var template = File.ReadAllText(templatePath);

        // Ersätt $NAME$
        template = template.Replace("$NAME$", commonName);

        // Bygg alt_names sektion från sanList
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
}