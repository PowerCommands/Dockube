using PainKiller.CommandPrompt.CoreLib.Core.Presentation;
using PainKiller.CommandPrompt.CoreLib.Modules.ShellModule.Services;

namespace PainKiller.DockubeClient.Commands;

[CommandDesign(description: "Dockube -  See detailed information about a resource (default is pod)",
                 arguments: ["<namespace>"],
                   options: ["pod", "svc", "ingress"],
               suggestions: ["gitlab"],
                  examples: ["//Describe a pod in namespace gitlab", "about gitlab",
                             "//Describe a service in namespace gitlab", "about gitlab --svc",
                             "//Describe an ingress in namespace gitlab", "about gitlab --ingress"])]
public class AboutCommand(string identifier) : ConsoleCommandBase<CommandPromptConfiguration>(identifier)
{
    public override RunResult Run(ICommandLineInput input)
    {
        var ns = input.Arguments.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(ns))
            return Nok("Namespace is required.");

        string kind;
        if (input.HasOption("svc"))
            kind = "svc";
        else if (input.HasOption("ingress"))
            kind = "ingress";
        else
            kind = "pod"; // default

        var rows = ShellService.Default.StartInteractiveProcess("kubectl", $"get {kind} -n {ns}").Split('\n');
        if (rows.Length <= 1)
        {
            Writer.WriteLine($"No {kind} found in namespace {ns}.");
            return Nok();
        }

        var items = rows.Select(r => $"{r.Split(' ').FirstOrDefault()}").Where(p => !string.IsNullOrWhiteSpace(p) && !p.Equals("NAME", StringComparison.OrdinalIgnoreCase)).ToList();

        var selected = ListService.ListDialog($"Select a {kind} to describe", items);
        var name = selected.First().Value;

        var result = ShellService.Default.StartInteractiveProcess("kubectl", $"describe {kind} {name} -n {ns}");
        Console.WriteLine(result);

        return Ok();
    }
}
