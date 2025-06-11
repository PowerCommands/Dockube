namespace PainKiller.DockubeClient.Commands;

[CommandDesign(     description: "Publish an release to your kubernetes cluster", 
                      arguments: ["<release name>"],
                        options: [""],
                    suggestions: ["Ingress-Nginx-Helm","Grafana-Prometheus"],
                       examples: ["//Publish Grafana-Prometheus your core cluster","publish Grafana-Prometheus"])]
public class PublishCommand(string identifier) : ConsoleCommandBase<CommandPromptConfiguration>(identifier)
{
    public override RunResult Run(ICommandLineInput input)
    {
        Environment.CurrentDirectory = AppContext.BaseDirectory;
        var releaseName = input.Arguments.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(releaseName)) return Nok("Release name is required. Please specify the release you want to publish.");
        Publish(releaseName);
        return Ok();
    }
    private RunResult Publish(string releaseName)
    {
        var release = Configuration.Dockube.Releases.FirstOrDefault(r => r.Name.Equals(releaseName, StringComparison.OrdinalIgnoreCase));
        if (release == null) return Nok($"Release '{releaseName}' not found in configuration.");
        
        var service = new PublishService(Configuration.Dockube.ManifestBasePath);
        service.ExecuteRelease(release);
        return Ok();
    }
}