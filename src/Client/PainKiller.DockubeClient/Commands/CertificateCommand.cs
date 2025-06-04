using PainKiller.CommandPrompt.CoreLib.Core.Presentation;

namespace PainKiller.DockubeClient.Commands;

[CommandDesign(     description: "Create and manage certificates", 
                         quotes: ["name"],
                      arguments: ["Operation"],
                        options: ["ca","days"],
                    suggestions: ["root", "intermediate","tls", "auth"],
                       examples: ["//Create root certificate using default name","certificate --root", "//Sign certificate using root certificate using default name","certificate --sign"])]
public class CertificateCommand(string identifier) : ConsoleCommandBase<CommandPromptConfiguration>(identifier)
{
    public override RunResult Run(ICommandLineInput input)
    {
        var output = Configuration.Dockube.Ssl.Output;
        ISslService sslService = new SslService(Configuration.Dockube.Ssl.ExecutablePath);
        var name = input.Quotes.FirstOrDefault() ?? Configuration.Dockube.Ssl.DefaultName;
        var caName = input.Options.TryGetValue("ca", out var ca) ? ca : Configuration.Dockube.Ssl.DefaultCa;
        var validDays = input.Options.TryGetValue("days", out var days) ? int.Parse(days) : 3650; // Default to 10 years
        var operation = this.GetSuggestion(input.Arguments.FirstOrDefault(), "root");
        if (operation == "root") return CreateRootCa(sslService, output, $"{name} Root CA", validDays);
        if (operation == "intermediate") return CreateIntermediateCa(sslService, output, $"{name} Intermediate CA", validDays);
        if (operation == "tls") return SignTslCertificate(sslService, output, name, validDays, caName);
        if (operation == "auth") return SignAuthCertificate(sslService, output, name, validDays, caName);

        return Ok();
    }
    private RunResult CreateRootCa(ISslService service, string output, string name, int validDays)
    {
        var result = service.CreateRootCertificate(name, validDays, output);
        Writer.WriteLine(result);
        return Ok();
    }
    private RunResult CreateIntermediateCa(ISslService service, string output, string name, int validDays)
    {
        var result = service.CreateIntermediateCertificate(name, validDays, output);
        Writer.WriteLine(result);
        return Ok();
    }
    private RunResult SignTslCertificate(ISslService service, string output, string name, int validDays, string ca)
    {
        Writer.WriteLine("Add additional names (SAN) this certificate should be valid for? (e.g. 127.0.0.1)\nEnter a comma-separated list (e.g. localhost,127.0.0.1), or leave blank to skip:");
        var sanResponse = Console.ReadLine();
        var sanItems = sanResponse.Split(',').ToList();
        var result = service.CreateRequestForTls(name, output, sanItems);
        Writer.WriteLine(result);
        
        var signResponse = DialogService.YesNoDialog($"Do you want to create and sign this certificate with {ca}?");
        if (!signResponse) return Ok();
        var signResult = service.CreateAndSignCertificate(name, validDays, output, ca);
        Writer.WriteLine(signResult);
        return Ok();
    }
    private RunResult SignAuthCertificate(ISslService service, string output, string name, int validDays, string ca)
    {
        Writer.WriteLine("Add additional names (SAN) this certificate should be valid for? (e.g. 127.0.0.1) ");
        var sanResponse = DialogService.QuestionAnswerDialog("Enter a comma-separated list (e.g. localhost,127.0.0.1), or leave blank to skip:");
        var sanItems = sanResponse.Split(',').ToList();
        var result = service.CreateRequestForAuth(name, output, sanItems);
        Writer.WriteLine(result);
        
        var signResponse = DialogService.YesNoDialog($"Do you want to create and sign this certificate with {ca}?");
        if (!signResponse) return Ok();
        var signResult = service.CreateAndSignCertificate(name, validDays, output, ca, sanItems);
        Writer.WriteLine(signResult);
        return Ok();
    }
}