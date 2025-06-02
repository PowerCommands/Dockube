namespace DockubeCommands.Configuration;

public class DockcubeConstants
{
    public string GitEmailEnvVar { get; set; } = "gitEmail";
    public string GitAccessTokenSecret { get; set; } = "gitAT";
    public string GogsTemplateFileName { get; set; } = "gogs-04-config-map.yaml";
    public string GogsManifestDirectory { get; set; } = "Manifests\\gogs";
    public string GogsNamespace { get; set; } = "gogs";
    public string GogsContainerStartsWith { get; set; } = "gogs-";
    public string ArgoCdTemplateFileName { get; set; } = "argocd-05-add-application-dockube.yaml";
    public string ArgoCdManifestDirectory { get; set; } = "Manifests\\argocd";
    public string ArgoCdNamespace { get; set; } = "argocd";
    public string MsSqlManifestDirectory { get; set; } = "Manifests\\ms-sql";
    public string RepositoryPath { get; set; } = "manifests";
    public string RepositoryPathPlaceholder { get; set; } = "##RepositoryPath##";
    public string RepositoryUrlPlaceholder { get; set; } = "##RepositoryUrl##";
}