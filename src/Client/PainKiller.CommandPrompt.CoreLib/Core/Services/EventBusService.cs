using Microsoft.Extensions.Logging;
using PainKiller.CommandPrompt.CoreLib.Logging.Services;

namespace PainKiller.CommandPrompt.CoreLib.Core.Services;
public class EventBusService : IEventBusService
{
    private readonly ILogger<EventBusService> _logger = LoggerProvider.CreateLogger<EventBusService>();
    private EventBusService(){}

    private static readonly Lazy<IEventBusService> Lazy = new(() => new EventBusService());
    public static IEventBusService Service => Lazy.Value;

    private readonly Dictionary<Type, List<Delegate>> _subscribers = new();
    public void Subscribe<TEvent>(Action<TEvent> handler)
    {
        if (!_subscribers.TryGetValue(typeof(TEvent), out var handlers))
        {
            handlers = [];
            _subscribers[typeof(TEvent)] = handlers;
        }

        _logger.LogDebug($"{handler.Method.Name} subscription added");
        handlers.Add(handler);
    }
    public void Unsubscribe<TEvent>(Action<TEvent> handler)
    {
        if (_subscribers.TryGetValue(typeof(TEvent), out var handlers))
        {
            handlers.Remove(handler);
            _logger.LogDebug($"{handler.Method.Name} subscription removed");
        }
    }
    public void Publish<TEvent>(TEvent eventData)
    {
        _logger.LogDebug($"{eventData?.ToString()} published.");
        if (!_subscribers.TryGetValue(typeof(TEvent), out var handlers)) return;
        foreach (var handler in handlers) ((Action<TEvent>)handler)?.Invoke(eventData);
    }
}