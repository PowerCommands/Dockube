namespace PainKiller.DockubeClient.DomainObjects;

public class ResourceProfile
{
    public int Replicas { get; set; } = 1;
    public string CpuRequest { get; set; } = "100m";
    public string CpuLimit { get; set; } = "200m";
    public string MemoryRequest { get; set; } = "128Mi";
    public string MemoryLimit { get; set; } = "256Mi";
}