namespace DockubeCommands.DomainObjects;

public class ProcessMetadata
{
    public string Name { get; set; } = string.Empty;
    public string Args { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public bool UseShellExecute { get; set; }
    public bool WaitForExit { get; set; }
    public bool DisableOutputLogging { get; set; }
    public bool Base64Decode { get; set; }
    public int WaitSec { get; set; }
    public bool UseReadline { get; set; }
}