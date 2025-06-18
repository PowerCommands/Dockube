namespace DockubeApi.Configuration.DomainObjects;

public class LogConfiguration
{
    public LogConfiguration() { }
    public LogConfiguration(string fileName, string filePath, string restrictedToMinimumLevel, string rollingIntervall)
    {
        FileName = fileName;
        FilePath = filePath;
        RestrictedToMinimumLevel = restrictedToMinimumLevel;
        RollingIntervall = rollingIntervall;
    }
    public string FileName { get; set; } = "powercommands.log";
    public string FilePath { get; set; } = "logs";
    public string RollingIntervall { get; set; } = "Day";
    public string RestrictedToMinimumLevel { get; set; } = "Information";
}