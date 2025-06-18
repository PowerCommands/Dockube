using PainKiller.CommandPrompt.CoreLib.Core.Presentation;

namespace PainKiller.DockubeClient.Commands;

[CommandDesign(     description: "Create and manage certificates", 
                         quotes: ["name"],
                      arguments: ["Operation"],
                        options: ["ca","days"],
                    suggestions: ["inspect","root", "intermediate","tls", "auth", "pem"],
                       examples: ["//Create root certificate using default name","pki --root", "//Sign and create a TLS certificate using root certificate using default name","pki tls --sign"])]
public class PkiCommand(string identifier) : ConsoleCommandBase<CommandPromptConfiguration>(identifier)
{
    private readonly ISslService _sslService = SslService.Default;

    public override RunResult Run(ICommandLineInput input)
    {
        var output = Configuration.Dockube.Ssl.Output;
        var name = input.Quotes.FirstOrDefault() ?? Configuration.Dockube.Ssl.DefaultName;
        var caName = input.Options.TryGetValue("ca", out var ca) ? ca : Configuration.Dockube.Ssl.DefaultCa;
        var validDays = input.Options.TryGetValue("days", out var days) ? int.Parse(days) : 3650; // Default to 10 years
        var operation = this.GetSuggestion(input.Arguments.FirstOrDefault(), "root");
        if (operation == "root") return CreateRootCa( output, $"{name} Root CA", validDays);
        if (operation == "intermediate") return CreateIntermediateCa(output, $"{name} Intermediate CA", validDays);
        if (operation == "tls") return SignTslCertificate(output, name, validDays, caName);
        if (operation == "pem") return CreatePemFile(output, name, caName);
        if (operation == "auth") return SignAuthCertificate(output, name, validDays, caName);
        if (operation == "inspect") return Inspect(name);

        return Ok();
    }

    private RunResult Inspect(string name)
    {
        var certInfo = _sslService.InspectCertificate(name);
        if (certInfo != null)
        {
            Writer.WriteHeadLine($"Subject: {certInfo.SubjectCn}");
            Writer.WriteLine($"Issuer: {certInfo.Issuer}");
            Writer.WriteLine($"Valid From: {certInfo.ValidFrom}");
            Writer.WriteLine($"Valid To: {certInfo.ValidTo}");
            Writer.WriteLine($"Is Valid Now: {certInfo.IsValidNow}");
            Writer.WriteLine($"Thumbprint (SHA1): {certInfo.ThumbprintSha1}");
            if (!string.IsNullOrEmpty(certInfo.KeyUsage))
            {
                Writer.WriteLine($"Key Usage: {certInfo.KeyUsage}");
            }
            if (certInfo.ExtendedKeyUsages.Any())
            {
                Writer.WriteLine("Extended Key Usages:");
                foreach (var usage in certInfo.ExtendedKeyUsages)
                {
                    Writer.WriteLine($"- {usage}");
                }
            }
            if (certInfo.SubjectAlternativeNames.Any())
            {
                Writer.WriteLine("Subject Alternative Names:");
                foreach (var san in certInfo.SubjectAlternativeNames)
                {
                    Writer.WriteLine($"- {san}");
                }
            }
        }
        return Ok();
    }
    private RunResult CreateRootCa(string output, string name, int validDays)
    {
        var result = _sslService.CreateRootCertificate(name, validDays, output);
        Writer.WriteLine(result);
        return Ok();
    }
    private RunResult CreateIntermediateCa(string output, string name, int validDays)
    {
        var result = _sslService.CreateIntermediateCertificate(name, validDays, output);
        Writer.WriteLine(result);
        return Ok();
    }
    private RunResult SignTslCertificate(string output, string name, int validDays, string ca)
    {
        Writer.WriteLine("Add additional names (SAN) this certificate should be valid for? (e.g. 127.0.0.1)\nEnter a comma-separated list (e.g. localhost,127.0.0.1), or leave blank to skip:");
        var sanResponse = Console.ReadLine();
        var sanItems = sanResponse.Split(',').ToList();
        var result = _sslService.CreateRequestForTls(name, output, sanItems);
        Writer.WriteLine(result);
        
        var signResponse = DialogService.YesNoDialog($"Do you want to create and sign this certificate with {ca}?");
        if (!signResponse) return Ok();
        var signResult = _sslService.CreateAndSignCertificate(name, validDays, output, ca);
        Writer.WriteLine(signResult);
        var pfxResult = _sslService.ExportToPfx(name, ca, output, password: "myPazzw0rd");
        Writer.WriteLine(pfxResult);
        return Ok();
    }
    private RunResult SignAuthCertificate(string output, string name, int validDays, string ca)
    {
        Writer.WriteLine("Add additional names (SAN) this certificate should be valid for? (e.g. 127.0.0.1) ");
        var sanResponse = DialogService.QuestionAnswerDialog("Enter a comma-separated list (e.g. localhost,127.0.0.1), or leave blank to skip:");
        var sanItems = sanResponse.Split(',').ToList();
        var result = _sslService.CreateRequestForAuth(name, output, sanItems);
        Writer.WriteLine(result);
        
        var signResponse = DialogService.YesNoDialog($"Do you want to create and sign this certificate with {ca}?");
        if (!signResponse) return Ok();
        var signResult = _sslService.CreateAndSignCertificate(name, validDays, output, ca, sanItems);
        Writer.WriteLine(signResult);
        return Ok();
    }
    private RunResult CreatePemFile(string output, string name, string ca)
    {
        var response = _sslService.ExportFullChainPemFile(name, ca, output);
        Writer.WriteLine(response);
        return Ok();
    }
}