using System.Text.RegularExpressions;
namespace PainKiller.CommandPrompt.CoreLib.Core.Runtime;
public class InputParser : IInputParser
{
    private static readonly Regex TokenPattern = new(@"--\w+(=""[^""]*""|\S+)?|""[^""]*""|\S+", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    public ICommandLineInput Parse(string rawInput)
    {
        rawInput = rawInput?.Trim() ?? string.Empty;
        if (rawInput.Length == 0) return new CommandLineInput(Raw: string.Empty, Identifier: string.Empty, Arguments: [], Quotes: [], Options: new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));

        var options = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var arguments = new List<string>();
        var quotes = new List<string>();

        var matches = TokenPattern.Matches(rawInput);
        if (matches.Count == 0) return new CommandLineInput(Raw: rawInput, Identifier: rawInput, Arguments: [], Quotes: [], Options: options);
        var identifier = matches[0].Value;
        for (var i = 1; i < matches.Count; i++)
        {
            var token = matches[i].Value;
            if (token.StartsWith("--", StringComparison.Ordinal))
            {
                var withoutDashes = token.Substring(2);
                if (withoutDashes.Contains('='))
                {
                    var idx = withoutDashes.IndexOf('=');
                    var key = withoutDashes.Substring(0, idx);
                    var rawVal = withoutDashes.Substring(idx + 1);
                    var val = TrimQuotes(rawVal);
                    options[key] = val;
                }
                else
                {
                    options[withoutDashes] = "";
                }
            }
            else
            {
                var val = TrimQuotes(token);
                if (token.Length > 1 && token[0] == '"' && token[^1] == '"') quotes.Add(val);
                arguments.Add(val);
            }
        }
        return new CommandLineInput(Raw: rawInput, Identifier: identifier, Arguments: arguments.ToArray(), Quotes: quotes.ToArray(), Options: options);
        
        static string TrimQuotes(string s)
        {
            if (s.Length > 1 && s[0] == '"' && s[^1] == '"') return s.Substring(1, s.Length - 2);
            return s;
        }
    }
}