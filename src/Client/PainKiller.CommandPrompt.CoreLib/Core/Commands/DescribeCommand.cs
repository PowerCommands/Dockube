using PainKiller.CommandPrompt.CoreLib.Core.BaseClasses;
using PainKiller.CommandPrompt.CoreLib.Core.Events;
using PainKiller.CommandPrompt.CoreLib.Metadata.Attributes;
using PainKiller.CommandPrompt.CoreLib.Metadata;
using PainKiller.ReadLine.Managers;
using Spectre.Console;

namespace PainKiller.CommandPrompt.CoreLib.Core.Commands;

[CommandDesign("Displays detailed information about a specific command", arguments: ["Command identifier"])]
public class DescribeCommand : ConsoleCommandBase<ApplicationConfiguration>
{
    public DescribeCommand(string identifier) : base(identifier) => EventBusService.Service.Subscribe<CommandsDiscoveredEventArgs>(e => { SuggestionProviderManager.AppendContextBoundSuggestions(Identifier, e.Identifiers); });
    public override RunResult Run(ICommandLineInput input)
    {
        var commandId = input.Arguments.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(commandId))
            return Nok("You must specify a command to describe.");

        if (!MetadataRegistryService.ReaderInstance.All.TryGetValue(commandId, out var meta))
            return Nok($"No such command: {commandId}");

        var attr = meta;

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[bold underline]Command:[/] [blue]{meta.Identifier}[/]");
        AnsiConsole.MarkupLine($"[grey]{attr.Description}[/]");

        var table = new Table()
            .RoundedBorder()
            .Expand()
            .AddColumn("[grey]Field[/]")
            .AddColumn("[grey]Values[/]");

        void AddRow(string label, IEnumerable<string> values, bool prefixWithDash = false)
        {
            var content = values.Any()
                ? string.Join("\n", values.Select(v => v.StartsWith("//")
                    ? $"[green]{Markup.Escape(v)}[/]"
                    : (prefixWithDash ? $"--{Markup.Escape(v)}" : Markup.Escape(v))))
                : "[grey](none)[/]";
            table.AddRow($"[bold]{label}[/]", content);
        }

        table.AddEmptyRow();
        AddRow("Arguments", attr.Arguments);
        AddRow("Options", attr.Options, prefixWithDash: true);  // Options alltid med "--" framför
        AddRow("Quotes", attr.Quotes);
        AddRow("Examples", attr.Examples);
        AddRow("Suggestions", attr.Suggestions);

        AnsiConsole.Write(table);
        return Ok();
    }
}
