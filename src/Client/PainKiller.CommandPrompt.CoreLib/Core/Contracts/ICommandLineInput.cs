namespace PainKiller.CommandPrompt.CoreLib.Core.Contracts;
public interface ICommandLineInput
{
    /// <summary>
    /// The raw, unmodified input from the user.
    /// </summary>
    string Raw { get; }

    /// <summary>
    /// The command identifier.
    /// </summary>
    string Identifier { get; }

    /// <summary>
    /// The array of standalone arguments (excluding flags and options), e.g., ["Jonas", "Smith"].
    /// </summary>
    string[] Arguments { get; }

    /// <summary>
    /// All parts of the input that were enclosed in quotes.
    /// </summary>
    string[] Quotes { get; }

    /// <summary>
    /// A dictionary of flags/options, where the key is the option's name (e.g., "name"),
    /// and the value is whatever immediately follows that option, or null if there is none.
    /// </summary>
    IDictionary<string, string> Options { get; }
}
