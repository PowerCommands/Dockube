using System.Globalization;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using PainKiller.CommandPrompt.CoreLib.Core.Runtime;
using PainKiller.CommandPrompt.CoreLib.Logging.Services;
using PainKiller.CommandPrompt.CoreLib.Metadata.DomainObjects;
using PainKiller.CommandPrompt.CoreLib.Metadata.Extensions;

namespace PainKiller.CommandPrompt.CoreLib.Core.Extensions;

public static class ConsoleCommandExtensions
{
    /// <summary>
    /// Executes the specified console command by providing structured input components.
    /// </summary>
    /// <param name="command">The console command to execute.</param>
    /// <param name="arguments">Optional list of arguments passed to the command (e.g. ["cd"]).</param>
    /// <param name="quotes">Optional quoted arguments (e.g. ["My Folder"]).</param>
    /// <param name="options">
    /// Optional options provided as key-value pairs (e.g. { "filter": "log", "json": "" }).
    /// For flag-style options, leave the value empty or null.
    /// </param>
    /// <returns>The result of executing the command.</returns>
    public static RunResult Execute(this IConsoleCommand command, string[]? arguments = null, string[]? quotes = null, Dictionary<string, string>? options = null)
    {
        ILogger<CommandRuntime> logger = LoggerProvider.CreateLogger<CommandRuntime>();
        logger.LogDebug($"{nameof(ConsoleCommandExtensions)}.{nameof(Execute)} {command.Identifier}");
        var input = new CommandLineInput(
            BuildRaw(command, arguments, quotes, options),
            command.Identifier,
            arguments ?? [],
            quotes ?? [],
            options ?? new());

        var executor = new CommandExecutor();
        return executor.Execute(command, input);
    }
    public static RunResult ExecuteWithSimpleOptions(this IConsoleCommand command, string[]? arguments = null, string[]? quotes = null, params string[] options)
    {
        var optionsDict = options.ToDictionary(opt => opt, _ => string.Empty);
        return command.Execute(arguments: arguments, quotes: quotes, options: optionsDict);
    }
    private static string BuildRaw(
        IConsoleCommand command,
        string[]? arguments,
        string[]? quotes,
        Dictionary<string, string>? options)
    {
        var parts = new List<string> { command.Identifier };

        if (arguments is { Length: > 0 })
            parts.AddRange(arguments);

        if (quotes is { Length: > 0 })
            parts.AddRange(quotes.Select(q => $"\"{q}\""));

        if (options is { Count: > 0 })
            parts.AddRange(options.Select(kv => $"--{kv.Key} {(string.IsNullOrWhiteSpace(kv.Value) ? "" : kv.Value)}"));

        return string.Join(" ", parts);
    }
    public static bool HasOption(this ICommandLineInput input, string option) => input.Options.ContainsKey(option.ToLower());
    public static string GetOptionValue(this ICommandLineInput input, string option) => input.Options.TryGetValue(option.ToLower(), out var value) ? value : string.Empty;
    public static T GetTypedOptionValue<T>(this ICommandLineInput input, string option, string defaultValue)
    {
        input.Options.TryGetValue(option.ToLower(), out var rawValue);
        var stringValue = rawValue ?? defaultValue;
        if (typeof(T).IsEnum) return (T)Enum.Parse(typeof(T), stringValue, ignoreCase: true);
        return (T)Convert.ChangeType(stringValue, typeof(T), CultureInfo.InvariantCulture);
    }
    public static bool TryGetOption<T>(this ICommandLineInput input, out T value, T defaultValue = default!, [CallerArgumentExpression("value")] string optionName = null!)
    {
        try
        {
            optionName = optionName.Split(' ').Last().ToLowerInvariant();
            if (!input.Options.TryGetValue(optionName, out var raw))
            {
                value = defaultValue;
                return false;
            }
            if (typeof(T) == typeof(bool))
            {
                value = (T)(object)true;
                return true;
            }
            if (string.IsNullOrEmpty(raw))
            {
                value = defaultValue;
                return false;
            }
            if (typeof(T).IsEnum)
            {
                value = (T)Enum.Parse(typeof(T), raw, ignoreCase: true);
            }
            else
            {
                value = (T)Convert.ChangeType(raw, typeof(T), CultureInfo.InvariantCulture);
            }
            return true;
        }
        catch
        {
            value = defaultValue;
            return false;
        }
    }
    public static string GetSuggestion(this IConsoleCommand command, string? argument, string defaultValue)
    {
        var retVal = argument;
        var metaData = command.GetMetadata() ?? new CommandMetadata();
        if (string.IsNullOrEmpty(argument) || metaData.Suggestions.All(s => s != argument)) retVal = defaultValue;
        return retVal ?? defaultValue;
    }
    public static string GetFullPath(this ICommandLineInput input)
    {
        var path = input.Arguments.FirstOrDefault() ?? Environment.CurrentDirectory;
        if (!path.Contains('\\') && !Path.IsPathRooted(path))
            path = Path.Combine(Environment.CurrentDirectory, path);
        var retVal = Path.GetFullPath(path);
        return retVal.GetReplacedPlaceHolderPath();
    }
}