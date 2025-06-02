using PainKiller.CommandPrompt.CoreLib.Core.BaseClasses;
using PainKiller.CommandPrompt.CoreLib.Core.Contracts;
using PainKiller.CommandPrompt.CoreLib.Core.DomainObjects;
using PainKiller.CommandPrompt.CoreLib.Metadata.Attributes;
using PainKiller.DockubeClient.Configuration;
using PainKiller.DockubeClient.Contracts;
using PainKiller.DockubeClient.Services;

namespace PainKiller.DockubeClient.Commands;

[CommandDesign(     description: "CA command, create root certificate, sign certificate", 
                         quotes: ["name"],
                        options: ["root", "days"],
                       examples: ["//Crate root certificate","certificate"])]
public class CertificateCommand(string identifier) : ConsoleCommandBase<CommandPromptConfiguration>(identifier)
{
    public override RunResult Run(ICommandLineInput input)
    {
        var output = Configuration.Dockube.Ssl.Output;
        ISslService sslService = new SslService(Configuration.Dockube.Ssl.ExecutablePath);
        var name = input.Quotes.FirstOrDefault() ?? "Dockube Root CA";
        var validDays = input.Options.TryGetValue("days", out var days) ? int.Parse(days) : 3650; // Default to 10 years
        sslService.CreateRootCertificate(name, validDays, output);
        return Ok();
    }
}