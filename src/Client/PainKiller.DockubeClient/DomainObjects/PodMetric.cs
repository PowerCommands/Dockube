namespace PainKiller.DockubeClient.DomainObjects;

public class PodMetric
{
    public string Namespace { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Node { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int CpuMilliCores { get; set; }
    public int MemoryMiB { get; set; }

    public static PodMetric FromTopLine(string[] columns)
    {
        return new PodMetric
        {
            Namespace = columns[0],
            Name = columns[1],
            CpuMilliCores = ParseCpu(columns[2]),
            MemoryMiB = ParseMemory(columns[3])
        };
    }

    private static int ParseCpu(string raw) =>
        raw.EndsWith("m") ? int.Parse(raw[..^1]) : int.Parse(raw) * 1000;

    private static int ParseMemory(string raw) =>
        raw.EndsWith("Mi") ? int.Parse(raw[..^2]) :
        raw.EndsWith("Gi") ? int.Parse(raw[..^2]) * 1024 : 0;
}
