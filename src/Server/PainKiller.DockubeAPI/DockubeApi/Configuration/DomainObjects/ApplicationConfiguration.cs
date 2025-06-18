namespace DockubeApi.Configuration.DomainObjects;
public class ApplicationConfiguration
{
    public CoreConfiguration Core { get; set; } = new();
    public LogConfiguration Log { get; set; } = new(fileName: "dockubeapi.log", filePath: "logs", restrictedToMinimumLevel: "Information", rollingIntervall: "Day");    
}