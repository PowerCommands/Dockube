namespace DockubeCommands.Commands;

[PowerCommandDesign( description: "Start or stop the gitea server, default behaviour is start, to stop it just use the option --stop",
                         options: "stop",
                         example: "gitea")]
public class GiteaCommand : CommandBase<PowerCommandsConfiguration>
{
    public GiteaCommand(string identifier, PowerCommandsConfiguration configuration) : base(identifier, configuration) { }

    public override RunResult Run()
    {
        Directory.SetCurrentDirectory(Path.Combine(AppContext.BaseDirectory, "Manifests", "gitea"));
        ShellService.Service.Execute("docker", HasOption("stop") ? "compose down" : "compose up -d", workingDirectory: "", WriteLine);
        return Ok();
    }
}