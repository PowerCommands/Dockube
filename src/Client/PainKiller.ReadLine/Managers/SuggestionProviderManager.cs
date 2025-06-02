using PainKiller.ReadLine.Extensions;

namespace PainKiller.ReadLine.Managers
{
    public class SuggestionProviderManager
    {
        private static readonly Dictionary<string, string[]> ContextBoundSuggestions = new();

        public Func<string, string[]> SuggestionProviderFunc;
        public SuggestionProviderManager() => SuggestionProviderFunc = GetSuggestions;
        public static void AddContextBoundSuggestions(string contextId, string[] suggestions)
        {
            var excludeComments = suggestions.Where(s => !s.StartsWith("--//")).Select(s => s.Replace("!", "")).ToArray();
            if (!ContextBoundSuggestions.ContainsKey(contextId)) ContextBoundSuggestions.Add(contextId, excludeComments);
        }

        public static void AppendContextBoundSuggestions(string contextId, string[] suggestions, bool clearAllExceptOptions = true)
        {
            var keepValues = ContextBoundSuggestions.TryGetValue(contextId, out var values)
                ? values.Where(v => v.StartsWith("--")).ToList()
                : new List<string>();

            keepValues.AddRange(suggestions.Distinct());

            ContextBoundSuggestions[contextId] = keepValues.ToArray();
        }
        private static string[] GetSuggestions(string input)
        {
            try
            {
                var inputs = input.Split(" ").ToList();
                if (inputs.Count < 2) return null!;
                var contextId = inputs.First();
                inputs.RemoveAt(0);

                if (!ContextBoundSuggestions.ContainsKey(contextId)) return null!;

                if (string.IsNullOrEmpty(inputs.Last())) return ContextBoundSuggestions.Where(c => c.Key == contextId).Select(c => c.Value).First().SortSuggestions();

                if (ContextBoundSuggestions.ContainsKey(contextId) && ContextBoundSuggestions.Any(c => c.Value.Any(v => v.ToLower().StartsWith(inputs.Last().ToLower().Substring(0, 1)))))
                {
                    var suggestions = ContextBoundSuggestions.Where(c => c.Key == contextId && c.Value.Any(v => v.Length > 0 && v.ToLower().StartsWith(inputs.Last().ToLower().Substring(0, 1))))
                        .Select(x => x.Value).FirstOrDefault();
                    if (suggestions == null) return null!;
                    return suggestions.Where(s => s.ToLower().StartsWith(inputs.Last().ToLower())).ToArray();
                }

                var buildPath = new List<string>();
                var startOfPathNotFound = true;
                foreach (var inputFragment in inputs)
                {
                    if (inputFragment.Contains(":\\")) startOfPathNotFound = false;
                    if (!startOfPathNotFound) buildPath.Add(inputFragment);
                }
                var filePath = string.Join(" ", buildPath).Replace("\"", "");
                if (!Directory.Exists(filePath)) return null!;

                var directoryInfo = new DirectoryInfo(filePath);
                var filter = directoryInfo.GetDirectories().Select(f => f.Name.Substring(0, f.Name.Length)).ToList();
                var fileFilter = directoryInfo.GetFiles().Select(f => f.Name.Substring(0, f.Name.Length)).ToArray();
                filter.AddRange(fileFilter);
                var retVal = filter.ToArray();
                return retVal;
            }
            catch (Exception e)
            {
                return new[] { e.Message };
            }
        }
    }
}