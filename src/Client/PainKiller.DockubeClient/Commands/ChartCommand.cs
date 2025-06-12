namespace PainKiller.DockubeClient.Commands;

[CommandDesign(     description: "Manage your Helm charts", 
                        options: ["update","delete"],
                    suggestions: ["nginx", "gitlab"],
                       examples: ["//View versions of a certain chart repo","chart nginx"])]
public class ChartCommand(string identifier) : ConsoleCommandBase<CommandPromptConfiguration>(identifier)
{
    public override RunResult Run(ICommandLineInput input)
    {
        var chartName = input.Arguments.Length > 0 ? input.Arguments[0] : null;

        if (string.IsNullOrEmpty(chartName)) return ShowRepos();

        if (input.HasOption("update"))
            return UpdateChart(chartName);
        if (input.HasOption("delete"))
            return DeleteChart(chartName);

        return ViewVersions(chartName);
    }
    public RunResult ShowRepos()
    {
        Writer.WriteHeadLine("Configured Helm repositories:");
        RunCommand("helm repo list", "Helm Repos");
        return Ok();
    }
    public RunResult ViewVersions(string chartName)
    {
        Writer.WriteHeadLine($"Available versions for '{chartName}':");
        RunCommand($"helm search repo {chartName} --versions", "Helm Versions");
        return Ok();
    }
    public RunResult UpdateChart(string chartName)
    {
        Writer.WriteHeadLine($"Updating Helm repository (and chart cache) for '{chartName}'...");
        RunCommand("helm repo update", "Helm Repo Update");
        return Ok();
    }
    public RunResult DeleteChart(string chartName)
    {
        Writer.WriteHeadLine($"Removing Helm repository entry '{chartName}'...");
        RunCommand($"helm repo remove {chartName}", "Helm Repo Remove");
        return Ok();
    }
}