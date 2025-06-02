namespace PainKiller.CommandPrompt.CoreLib.Modules.StorageModule.Contracts;

public interface IObjectStorage<T, TItem> where T : IDataObjects<TItem>, new() where TItem : class, new()
{
    List<TItem> GetItems(bool reload = false);
    void SaveItems(List<TItem> items);
    void Insert(TItem item, Func<TItem, bool> match, bool saveToFile = true);
    bool Remove(Func<TItem, bool> match, bool saveToFile = true);
    void Save();
}