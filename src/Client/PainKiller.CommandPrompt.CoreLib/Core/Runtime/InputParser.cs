using System.Text.RegularExpressions;

namespace PainKiller.CommandPrompt.CoreLib.Core.Runtime;

public class InputParser : IInputParser
{
    public ICommandLineInput Parse(string rawInput)
    {
        rawInput = rawInput?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(rawInput)) return new CommandLineInput(Raw: string.Empty, Identifier: string.Empty, Arguments: [], Quotes: [], Options: new Dictionary<string, string?>());
        
        var quotes = new List<string>();
        var quoteMatches = Regex.Matches(rawInput, "\"([^\"]*)\"");
        foreach (Match match in quoteMatches) quotes.Add(match.Value);
     
        var rawWithoutQuotes = rawInput;
        foreach (var q in quotes) rawWithoutQuotes = rawWithoutQuotes.Replace(q, string.Empty);
        var tokens = rawWithoutQuotes.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
        var identifier = tokens.Count > 0 ? tokens[0] : string.Empty;
        if (tokens.Count > 0) tokens.RemoveAt(0);
        var options = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        var arguments = new List<string>();
        for (int i = 0; i < tokens.Count; i++)
        {
            var token = tokens[i];
            if (token.StartsWith("--"))
            {
                var key = token.TrimStart('-');
                var value = string.Empty;

                if (i + 1 < tokens.Count && !tokens[i + 1].StartsWith("--"))
                {
                    value = tokens[i + 1];
                    i++;
                }
                options[key] = value;
            }
            else
            {
                arguments.Add(token);
            }
        }
        for (int i = 0; i < quotes.Count; i++) quotes[i] = quotes[i].Trim('"');
        return new CommandLineInput(Raw: rawInput, Identifier: identifier, Arguments: arguments.ToArray(), Quotes: quotes.ToArray(), Options: options);
    }
}