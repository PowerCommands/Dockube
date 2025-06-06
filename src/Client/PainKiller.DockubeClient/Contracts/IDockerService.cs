namespace PainKiller.DockubeClient.Contracts;

public interface IDockerService
{
    string EnsureDockerRunning();
    string Version { get; }
}