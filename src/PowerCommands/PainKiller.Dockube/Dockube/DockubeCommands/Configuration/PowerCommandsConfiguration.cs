namespace DockubeCommands.Configuration
{
    public class PowerCommandsConfiguration : CommandsConfiguration
    {
        //Here is the placeholder for your custom configuration, you need to add the change to the PowerCommandsConfiguration.yaml file as well
        public string DefaultGitRepositoryPath { get; set; } = "C:\\repos";
        public string PathToDockerDesktop { get; set; } = "http://192.168.0.15:3000/api/v1";
        public string GogsServer { get; set; } = "";
        public string GogsUserName { get; set; } = "";
        public string GogsMainRepo { get; set; } = "";

    }
}