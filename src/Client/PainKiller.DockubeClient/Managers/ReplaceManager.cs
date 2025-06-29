using PainKiller.CommandPrompt.CoreLib.Modules.SecurityModule.Services;
using System.Text;

namespace PainKiller.DockubeClient.Managers;

public static class ReplaceManager
{
    public static void DecryptSecrets(string templatePath, string outputPath, string releaseName, string source)
    {
        if (string.IsNullOrEmpty(source)) return;
        var templateFullPath = Path.Combine(outputPath, releaseName, source);
        if(!File.Exists(templateFullPath))
        {
            Console.WriteLine($"File {templateFullPath} does not exist, skipping decryption.");
            return;
        }
        var content = File.ReadAllText(templateFullPath);
        if (!content.Contains("<ENCRYPTED_STRING>")) return;

        var buildContent = new StringBuilder();
        var rows = content.Split('\n');

        foreach (var row in rows)
        {
            var updatedRow = row;
            var startTag = "<ENCRYPTED_STRING>";
            var endTag = "</ENCRYPTED_STRING>";

            int startIdx;
            while ((startIdx = updatedRow.IndexOf(startTag, StringComparison.Ordinal)) != -1)
            {
                var endIdx = updatedRow.IndexOf(endTag, startIdx + startTag.Length, StringComparison.Ordinal);
                if (endIdx == -1) break; // Malformed tag, ignore

                var encryptedValue = updatedRow.Substring(
                    startIdx + startTag.Length,
                    endIdx - startIdx - startTag.Length
                );

                var decryptedValue = EncryptionService.Service.DecryptString(encryptedValue);
                updatedRow = updatedRow.Substring(0, startIdx)
                             + decryptedValue
                             + updatedRow.Substring(endIdx + endTag.Length);
            }

            buildContent.AppendLine(updatedRow);
        }
        var fullPath = Path.Combine(outputPath, releaseName, source);
        File.WriteAllText(fullPath, buildContent.ToString());
    }
    public static void ReplacePlaceholdersInFile(string templatePath, string outputPath, Dictionary<string, string> placeholders)
    {
        if (!File.Exists(templatePath)) return;

        var content = File.ReadAllText(templatePath);

        foreach (var kvp in placeholders)
        {
            content = content.Replace(kvp.Key, kvp.Value);
        }
        File.WriteAllText(outputPath, content);
    }
}