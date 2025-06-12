namespace PainKiller.DockubeClient.Contracts;

public interface IPublishService
{
    void ExecuteRelease(DockubeRelease release);
    void UninstallRelease(DockubeRelease release);
}