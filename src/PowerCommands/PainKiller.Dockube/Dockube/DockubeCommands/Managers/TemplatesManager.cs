namespace DockubeCommands.Managers;
public static class TemplatesManager
{
    public static void FindReplaceFile(Dictionary<string,string> findAndReplaces, string templateFileFullName, string targetFileFullName)
    {
        if (!File.Exists(templateFileFullName)) return;
        var content = $"{File.ReadAllText(templateFileFullName)}";
        var replacedContent = findAndReplaces.Aggregate(content, (current, findAndReplace) => current.Replace(findAndReplace.Key, findAndReplace.Value));
        File.WriteAllText(targetFileFullName, replacedContent);
    }
}