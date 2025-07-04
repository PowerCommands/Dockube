using System.Text.Json;
using System.Text.RegularExpressions;
using PainKiller.CommandPrompt.CoreLib.Modules.ShellModule.Services;
using Spectre.Console;

namespace PainKiller.DockubeClient.Commands;

[CommandDesign(     description: "Dockube -  Show node metrics", 
                       examples: ["//Show metrics","metrics"])]
public class MetricsCommand(string identifier) : ConsoleCommandBase<CommandPromptConfiguration>(identifier)
{
    public override RunResult Run(ICommandLineInput input)
    {
        var podJson = ShellService.Default.StartInteractiveProcess("kubectl", "get pods -A -o json");
        var topOutput = ShellService.Default.StartInteractiveProcess("kubectl", "top pods -A");

        var metadata = new Dictionary<(string ns, string name), (string node, string status)>();

        using var doc = JsonDocument.Parse(podJson);
        var items = doc.RootElement.GetProperty("items");

        foreach (var item in items.EnumerateArray())
        {
            var ns = item.GetProperty("metadata").GetProperty("namespace").GetString() ?? "";
            var name = item.GetProperty("metadata").GetProperty("name").GetString() ?? "";
            var status = item.GetProperty("status").TryGetProperty("phase", out var statusProp) ? statusProp.GetString() ?? "" : "";
            var node = item.GetProperty("spec").TryGetProperty("nodeName", out var nodeProp) ? nodeProp.GetString() ?? "" : "";
            metadata[(ns, name)] = (node, status);
        }

        var metrics = new List<PodMetric>();
        foreach (var line in topOutput.Split('\n', StringSplitOptions.RemoveEmptyEntries).Skip(1))
        {
            var cols = Regex.Split(line.Trim(), @"\s+");
            if (cols.Length == 4)
            {
                var metric = PodMetric.FromTopLine(cols);
                if (metadata.TryGetValue((metric.Namespace, metric.Name), out var meta))
                {
                    metric.Node = meta.node;
                    metric.Status = meta.status;
                    metrics.Add(metric);
                }
            }
        }

        // Per-namespace output
        var grouped = metrics.GroupBy(m => m.Namespace).OrderBy(g => g.Key);
        foreach (var nsGroup in grouped)
        {
            var table = new Table().Border(TableBorder.Rounded).Title($"[bold underline green]{nsGroup.Key}[/]");
            table.AddColumn("Pod");
            table.AddColumn("Node");
            table.AddColumn("Status");
            table.AddColumn("CPU");
            table.AddColumn("Memory");

            foreach (var pod in nsGroup.OrderBy(p => p.Name))
            {
                table.AddRow(
                    pod.Name,
                    pod.Node,
                    FormatHelpers.FormatStatus(pod.Status ?? ""),
                    FormatHelpers.FormatCpu(pod.CpuMilliCores),
                    FormatHelpers.FormatMemory(pod.MemoryMiB)
                );
            }

            var nsCpu = nsGroup.Sum(p => p.CpuMilliCores);
            var nsMem = nsGroup.Sum(p => p.MemoryMiB);
            table.AddRow(
                "[bold yellow]Namespace Total[/]",
                "",
                "",
                $"[bold green]{FormatHelpers.FormatCpu(nsCpu)}[/]",
                $"[bold green]{FormatHelpers.FormatMemory(nsMem)}[/]"
            );

            AnsiConsole.Write(table);
            Writer.WriteLine();
        }

        // Node summary
        var byNode = metrics
            .Where(m => !string.IsNullOrWhiteSpace(m.Node))
            .GroupBy(m => m.Node)
            .OrderBy(n => n.Key);

        var summaryTable = new Table().Border(TableBorder.Rounded).Title("[bold underline green]Node Summary[/]");
        summaryTable.AddColumn("Node");
        summaryTable.AddColumn("Total CPU");
        summaryTable.AddColumn("Total Memory");

        foreach (var nodeGroup in byNode)
        {
            var cpu = nodeGroup.Sum(x => x.CpuMilliCores);
            var mem = nodeGroup.Sum(x => x.MemoryMiB);
            summaryTable.AddRow(
                nodeGroup.Key,
                $"[green]{FormatHelpers.FormatCpu(cpu)}[/]",
                $"[green]{FormatHelpers.FormatMemory(mem)}[/]"
            );
        }

        AnsiConsole.Write(summaryTable);
        return Ok();
    }
}





public static class FormatHelpers
{
    public static string FormatCpu(int millicores) =>
        millicores >= 1000 ? $"{millicores / 1000.0:F2} cores" : $"{millicores}m";

    public static string FormatMemory(int mib) =>
        mib >= 1024 ? $"{mib / 1024.0:F2} Gi" : $"{mib} Mi";

    public static string FormatStatus(string status) =>
        status switch
        {
            "Running" => $"[green]{status}[/]",
            "Completed" => $"[blue]{status}[/]",
            "CrashLoopBackOff" or "Error" or "ImagePullBackOff" => $"[red]{status}[/]",
            _ => $"[yellow]{status}[/]"
        };
}


//Install component if not installed
//kubectl apply -f https://github.com/kubernetes-sigs/metrics-server/releases/latest/download/components.yaml