namespace PainKiller.DockubeClient.DomainObjects;

public class SecretDescriptor
{
    public string Name { get; set; } = string.Empty;
    public string Key { get; set; } = "password";
    public bool ShowClearText { get; set; } = false;
}