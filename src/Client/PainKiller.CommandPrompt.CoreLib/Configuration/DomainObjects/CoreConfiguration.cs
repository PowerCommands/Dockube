namespace PainKiller.CommandPrompt.CoreLib.Configuration.DomainObjects;
public class CoreConfiguration
{
    private string _roamingDirectory = "";
    public string Name { get; set; } = "Command Prompt";
    public string Version { get; set; } = "1.0";
    public string Prompt { get; set; } = "pcm>";
    public string DefaultCommand { get; set; } = "";
    public bool ShowLogo { get; set; } = true;
    public string LogoColor { get; set; } = "DarkMagenta";
    public List<string> Suggestions { get; set; } = [];

    public string RoamingDirectory
    {
        get => Path.Combine(ApplicationConfiguration.CoreApplicationDataPath, _roamingDirectory);
        set => _roamingDirectory = value;
    }

    public ModulesConfiguration Modules { get; set; } = new();
}