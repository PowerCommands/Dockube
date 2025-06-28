using PainKiller.CommandPrompt.CoreLib.Core.Presentation;
using PainKiller.CommandPrompt.CoreLib.Modules.ShellModule.Services;

namespace PainKiller.DockubeClient.Commands;

[CommandDesign(     description: "Dockube -  See log on the provided item", 
                      arguments: ["<namespace>","<instance-name>"],
                        options: ["file", "prepare-data"],
                    suggestions: ["gitlab"],
                       examples: ["//View log on pod gitlab-c649d8bc-lhjjc in namespace gitlab","logs gitlab gitlab-c649d8bc-lhjjc"])]
public class LogsCommand(string identifier) : ConsoleCommandBase<CommandPromptConfiguration>(identifier)
{
    public override RunResult Run(ICommandLineInput input)
    {
        var ns = input.Arguments.First();
        if (string.IsNullOrWhiteSpace(ns)) return Nok("Namespace are required.");
        var rows = ShellService.Default.StartInteractiveProcess("kubectl", $"get pods -n {ns}").Split('\n');
        if (rows.Length == 0)
        {
            Writer.WriteLine($"No pods found in namespace {ns}.");
            return Nok();
        }
        
        var pods = rows.Select(r => $"{r.Split(' ').FirstOrDefault()}").Where(p => !string.IsNullOrEmpty(p.Trim()) && p.Trim().ToLower() != "name").ToList();
        var pod = ListService.ListDialog("Select your pod", pods);
        var podIdentity = pod.First().Value;

        var prepareData = "";
        if (input.HasOption("prepare-data"))
        {
            prepareData = ShellService.Default.StartInteractiveProcess("kubectl", $"logs -n {ns}  {podIdentity} -c prepare-data");
            if (!string.IsNullOrWhiteSpace(prepareData))
            {
                Writer.WriteLine($"Prepare Data Logs for {podIdentity} in namespace {ns}:");
                Writer.WriteLine(prepareData);
                Writer.WriteLine("--------------------------------------------------");
            }
        }
        
        var result = ShellService.Default.StartInteractiveProcess("kubectl", $"logs {podIdentity}  -n {ns}");
        Console.WriteLine(result);
        if (input.HasOption("file"))
        {
            var fileName = $"{podIdentity}-logs.txt".PrefixFileTimestamp();
            File.WriteAllText(fileName, result);
            Writer.WriteLine($"Logs saved to {fileName}");
            if (input.HasOption("prepare-data"))
            {
                var fileName2 = $"{podIdentity}-prepare-logs.txt".PrefixFileTimestamp();
                File.WriteAllText(fileName2, prepareData);
                Writer.WriteLine($"Logs saved to {fileName2}");
            }
        }
        return Ok();
    }
}