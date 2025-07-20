using PainKiller.DockubeClient.Extensions;
using PainKiller.ReadLine.Managers;

namespace PainKiller.DockubeClient.Commands;

[CommandDesign(     description: "Dockube -  Install an release to your kubernetes cluster", 
                      arguments: ["<release name>"],
                        options: ["uninstall"],
                       examples: ["//Publish Grafana-Prometheus your core cluster","publish Grafana-Prometheus"])]
public class InstallCommand(string identifier) : ConsoleCommandBase<CommandPromptConfiguration>(identifier)
{
    private readonly string _identifier = identifier;
    public override void OnInitialized()
    {
        SuggestionProviderManager.AppendContextBoundSuggestions(_identifier, Configuration.Dockube.GetReleases().Select(s => s.Name).ToArray());
        base.OnInitialized();
    }

    public override RunResult Run(ICommandLineInput input)
    {
        Environment.CurrentDirectory = AppContext.BaseDirectory;
        var releaseName = input.Arguments.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(releaseName)) return Nok("Release name is required. Please specify the release you want to publish.");
        var release = Configuration.Dockube.GetReleases().FirstOrDefault(r => r.Name.Equals(releaseName, StringComparison.OrdinalIgnoreCase));
        if (release == null) return Nok($"Release '{release?.Name}' not found in configuration.");
        var service = new PublishService(Configuration.Dockube.ManifestsPath, Configuration.Dockube.TemplatesPath, Configuration.Dockube.Ssl.Output, Configuration.Dockube.Ssl.DefaultCa, Configuration.Dockube.DefaultDomain);
        
        if (input.TryGetOption(out bool uninstall, false)) return UnInstall(release, service);
        
        Install(release, service);
        return Ok();
    }
    private RunResult Install(DockubeRelease release, IPublishService service)
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