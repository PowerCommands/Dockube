using PainKiller.CommandPrompt.CoreLib.Modules.InfoPanelModule.Contracts;
using PainKiller.DockubeClient.Managers;

namespace PainKiller.DockubeClient.DomainObjects;
public class DockubeInfoPanelContent(DockubeConfiguration configuration) : IInfoPanelContent
{
    public string GetText()
    {
        var sslVersion = GetSslVersion();
        var currentEnvironment = KubeEnvironmentManager.GetTarget();
        var version = KubeEnvironmentManager.GetVersion();
        var helmVersion = KubeEnvironmentManager.GetHelmVersion();

        var statuses = string.Join(", ", ServiceStatusManager.GetServicesStatus(configuration).Select(s => $"{s.Name} [green]{(s.IsAvailable ? "✅" : "[red]❌[/]")}[/]"));
        var domainLabel = $"Domain: {configuration.DefaultDomain}";
        var servicesLabel = $"Services: {statuses}";
        var environmentLabel = $"Target: {currentEnvironment}";
        var versionLabel = $"Kubernetes:  ({version})";
        var sslLabel = $"SSL: {sslVersion}";
        var helmVersionLabel = $"Helm: {helmVersion}";
        const int padding = -25;
        return $"|{domainLabel,padding}| {environmentLabel,padding}| {servicesLabel,padding}\n|{sslLabel,padding}| {helmVersionLabel, padding}| {versionLabel}";
    }
    private string GetSslVersion()
    {
        var versionInfo = SslService.Default.GetVersion().Trim().Split(' ').Take(2);
        var retVal = $"{string.Join(' ', versionInfo).Trim()}";
        return retVal;
    }
}