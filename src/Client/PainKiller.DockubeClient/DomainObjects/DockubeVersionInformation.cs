namespace PainKiller.DockubeClient.DomainObjects;
public record DockubeVersionInformation(string SslVersion, string KubeVersion, string HelmVersion, string CurrentEnvironment);