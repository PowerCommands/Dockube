namespace DockubeCommands.Configuration;
public class PowerCommandsConfiguration : CommandsConfiguration
{
    public string PathToDockerDesktop { get; set; } = "http://192.168.0.15:3000/api/v1";
    public string GitServerApi { get; set; } = "";
    public string GitServer { get; set; } = "";
    public string GitUserName { get; set; } = "";
    public string GitMainRepo { get; set; } = "";

}