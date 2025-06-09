using PainKiller.DockubeClient.DomainObjects;

namespace PainKiller.DockubeClient.Contracts;

public interface IPublishService
{
    void ExecuteRelease(DockubeRelease release);
}