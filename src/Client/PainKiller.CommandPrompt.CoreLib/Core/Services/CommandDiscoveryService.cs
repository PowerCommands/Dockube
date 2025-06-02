using System.Reflection;
using PainKiller.CommandPrompt.CoreLib.Core.Events;
using PainKiller.CommandPrompt.CoreLib.Metadata;

namespace PainKiller.CommandPrompt.CoreLib.Core.Services;

public static class CommandDiscoveryService
{
    private static List<IConsoleCommand>? _cache;
    public static List<IConsoleCommand> DiscoverCommands(object? configuration = null)
    {
        if (_cache != null) return _cache;
        var metadataRegistry = MetadataRegistryService.WritableInstance;
        _cache = new List<IConsoleCommand>();

        var types = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => typeof(IConsoleCommand).IsAssignableFrom(t) && !t.IsAbstract && t.Name.EndsWith("Command"));
        foreach (var type in types)
        {
            var identifier = type.Name[..^7].ToLowerInvariant();
            var ctor = type.GetConstructor([typeof(string)]);
            if (ctor is null) continue;

            var instance = ctor.Invoke([identifier]);

            if (configuration != null)
            {
                var baseType = type.BaseType;
                var setMethod = baseType?.GetMethod("SetConfiguration", BindingFlags.Instance | BindingFlags.NonPublic);
                var configProperty = baseType?.GetProperty("Configuration", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

                if (setMethod != null && configProperty != null && configProperty.PropertyType.IsInstanceOfType(configuration))
                {
                    setMethod.Invoke(instance, [configuration]);
                }
            }
            var command = (IConsoleCommand)instance;
            if (_cache.Any(c => c.Identifier == command.Identifier)) continue;
            _cache.Add(command);
            metadataRegistry.Register(command);
        }
        EventBusService.Service.Publish(new CommandsDiscoveredEventArgs(_cache.Select(c => c.Identifier).ToArray()));
        return _cache;
    }

    /// <summary>
    /// Attempts to retrieve an instantiated command by its identifier.
    /// Returns false if the command is not found or if the discovery cache is uninitialized.
    /// </summary>

    public static bool TryGetCommand(string identifier, out IConsoleCommand? command)
    {
        if (_cache == null)
        {
            command = null;
            return false;
        }
        command = _cache.FirstOrDefault(c => c.Identifier.Equals(identifier, StringComparison.OrdinalIgnoreCase));
        return command != null;
    }
}