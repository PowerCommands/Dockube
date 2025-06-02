namespace DockubeCommands.Contracts;

public interface IPublishManager
{
    string Publish(string path, string kubernetesNamespace);
}