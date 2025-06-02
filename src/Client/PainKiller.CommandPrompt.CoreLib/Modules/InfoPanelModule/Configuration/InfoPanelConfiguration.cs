namespace PainKiller.CommandPrompt.CoreLib.Modules.InfoPanelModule.Configuration;

public class InfoPanelConfiguration
{
    public bool Enabled { get; set; }
    public int Height { get; set; } = 3;
    public int UpdateIntervalSeconds { get; set; } = -1;
    public string BackgroundColor { get; set; } = "";
    public string ForegroundColor { get; set; } = "";
}