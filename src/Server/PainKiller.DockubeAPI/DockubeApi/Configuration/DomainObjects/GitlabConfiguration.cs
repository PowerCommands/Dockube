namespace DockubeApi.Configuration.DomainObjects;
public class GitlabConfiguration
{
    public string BaseUrl { get; set; } = "https://gitlab.dockube.lan";
    public string Repository { get; set; } = "Dockube";
    public string UserName { get; set; } = "dockube@dockube.lan";
    public string AccessToken { get; set; } = "";    
}