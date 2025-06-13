using PainKiller.DockubeClient.Extensions;

namespace PainKiller.DockubeClient.Commands;

[CommandDesign(     description: "Publish an release to your kubernetes cluster", 
                      arguments: ["<release name>"],
                        options: ["uninstall"],
                    suggestions: ["Ingress-Nginx-Helm","Grafana","Prometheus","Minio","Gitlab"],
                       examples: ["//Publish Grafana-Prometheus your core cluster","publish Grafana-Prometheus"])]
public class PublishCommand(string identifier) : ConsoleCommandBase<CommandPromptConfiguration>(identifier)
{
    public override RunResult Run(ICommandLineInput input)
    {
        Environment.CurrentDirectory = AppContext.BaseDirectory;
        var releaseName = input.Arguments.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(releaseName)) return Nok("Release name is required. Please specify the release you want to publish.");
        var release = Configuration.Dockube.GetReleases().FirstOrDefault(r => r.Name.Equals(releaseName, StringComparison.OrdinalIgnoreCase));
        if (release == null) return Nok($"Release '{release?.Name}' not found in configuration.");
        var service = new PublishService(Configuration.Dockube.ManifestBasePath, Configuration.Dockube.Ssl.Output, Configuration.Dockube.Ssl.DefaultCa);
        
        if (input.TryGetOption(out bool uninstall, false)) return UnInstall(release, service);
        
        Publish(release, service);
        return Ok();
    }
    private RunResult Publish(DockubeRelease release, IPublishService service)
    {
        service.ExecuteRelease(release);
        return Ok();
    }
    private RunResult UnInstall(DockubeRelease release, IPublishService service)
    {
        service.UninstallRelease(release);
        return Ok();
    }

}