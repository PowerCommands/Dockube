namespace DockubeCommands.Managers;
public static class TemplatesManager
{
    public static void FindReplaceFile(Dictionary<string,string> findAndReplaces, string fileName, string targetDirectory)
    {
        var templateFileFullName = Path.Combine(AppContext.BaseDirectory, "Manifests", "templates", fileName);
        if (!File.Exists(templateFileFullName)) return;
        var targetFileFullName = Path.Combine(targetDirectory, fileName);
        var content = $"{File.ReadAllText(templateFileFullName)}";
        var replacedContent = findAndReplaces.Aggregate(content, (current, findAndReplace) => current.Replace(findAndReplace.Key, findAndReplace.Value));
        File.WriteAllText(targetFileFullName, replacedContent);
    }
}