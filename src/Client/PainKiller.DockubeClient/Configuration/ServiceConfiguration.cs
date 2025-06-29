namespace PainKiller.DockubeClient.Configuration;

public class ServiceConfiguration
{
    public string Host { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool Tsl { get; set; } = true;
    public bool IsAvailable { get; private set; }
    public void SetIsAvailable(bool available) => IsAvailable = available;
}