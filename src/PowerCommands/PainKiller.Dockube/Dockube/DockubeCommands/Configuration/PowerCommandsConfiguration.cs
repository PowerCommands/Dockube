namespace DockubeCommands.Configuration;
public class PowerCommandsConfiguration : CommandsConfiguration
{
    public string PathToDockerDesktop { get; set; } = "";
    public string GitServerApi { get; set; } = "";
    public string GitServer { get; set; } = "";
    public string GitUserName { get; set; } = "";
    public string GitMainRepo { get; set; } = "";
    public DockcubeConstants Constants { get; set; } = new();

}