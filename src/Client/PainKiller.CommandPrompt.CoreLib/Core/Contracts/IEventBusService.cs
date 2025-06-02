namespace PainKiller.CommandPrompt.CoreLib.Core.Contracts;

public interface IEventBusService
{
    void Subscribe<TEvent>(Action<TEvent> handler);
    void Unsubscribe<TEvent>(Action<TEvent> handler);
    void Publish<TEvent>(TEvent eventData);
}