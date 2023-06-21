namespace DockubeCommands.Commands;

[PowerCommandDesign(description: "Startup docker desktop",
                         example: "dd")]
public class DdCommand : CommandBase<PowerCommandsConfiguration>
{
    public DdCommand(string identifier, PowerCommandsConfiguration configuration) : base(identifier, configuration) { }

    public override RunResult Run()
    {
        Console.Clear();
        WriteSuccessLine("Docker Desktop starting please wait...");
        var fullFileName = Path.Combine(Configuration.PathToDockerDesktop, "Docker Desktop.exe");
        ShellService.Service.Execute(fullFileName, arguments: "", workingDirectory: "", WriteLine, fileExtension: "");
        Thread.Sleep(15000);
        WriteSuccessLine("Docker Desktop started");
        return Ok();
    }
}