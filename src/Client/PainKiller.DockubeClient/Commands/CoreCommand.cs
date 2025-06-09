namespace PainKiller.DockubeClient.Commands;

[CommandDesign(     description: "Manage the Core kubernetes cluster", 
                      arguments: ["<Mode>"],
                        options: ["valuesFile"],
                    suggestions: ["init","apply"],
                       examples: ["//View status of your core cluster","core","//Initialize the core cluster"])]
public class CoreCommand(string identifier) : ConsoleCommandBase<CommandPromptConfiguration>(identifier)
{
    public override RunResult Run(ICommandLineInput input)
    {
        return Ok();
    }
    private RunResult InitCoreCluster()
    {
        Writer.WriteHeadLine("Initializing Core Cluster...");
        return Ok();
    }
}