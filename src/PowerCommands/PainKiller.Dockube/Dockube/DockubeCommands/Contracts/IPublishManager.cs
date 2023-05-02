namespace DockubeCommands.Contracts;

public interface IPublishManager
{
    void Publish(string path, string kubernetesNamespace);
}