using PainKiller.CommandPrompt.CoreLib.Modules.InfoPanelModule.Contracts;
using PainKiller.DockubeClient.Managers;

namespace PainKiller.DockubeClient.DomainObjects;
public class DockubeInfoPanelContent(DockubeConfiguration configuration) : IInfoPanelContent
{
    public string GetText()
    {
        var versionInfo = VersionInformationManager.GetVersionInformation();

        var statuses = string.Join(", ", ServiceStatusManager.GetServicesStatus(configuration).Select(s => $"{s.Name} [green]{(s.IsAvailable ? "✅" : "[red]❌[/]")}[/]"));
        var domainLabel = $"Domain: {configuration.DefaultDomain}";
        var servicesLabel = $"Services: {statuses}";
        var environmentLabel = $"Target: {versionInfo.CurrentEnvironment}";
        var versionLabel = $"Kubernetes:  ({versionInfo.KubeVersion})";
        var sslLabel = $"SSL: {versionInfo.SslVersion}";
        var helmVersionLabel = $"Helm: {versionInfo.HelmVersion}";
        const int padding = -25;
        return $"|{domainLabel,padding}| {environmentLabel,padding}| {servicesLabel,padding}\n|{sslLabel,padding}| {helmVersionLabel, padding}| {versionLabel}";
    }
}

