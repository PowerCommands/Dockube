using PainKiller.CommandPrompt.CoreLib.Modules.StorageModule.Contracts;
using PainKiller.CommandPrompt.CoreLib.Modules.StorageModule.Services;
namespace PainKiller.CommandPrompt.CoreLib.Modules.StorageModule.DomainObjects;
public class ObjectStorage<T, TItem> : IObjectStorage<T, TItem> where T : IDataObjects<TItem>, new() where TItem : class, new()
{
    protected T DataObject = StorageService<T>.Service.GetObject();

    public virtual List<TItem> GetItems(bool reload = false)
    {
        if (reload) DataObject = StorageService<T>.Service.GetObject();
        return DataObject.Items;
    }
    public virtual void SaveItems(List<TItem> items)
    {
        DataObject.Items = items;
        DataObject.LastUpdated = DateTime.Now;
        StorageService<T>.Service.StoreObject(DataObject);
    }
    public virtual void Insert(TItem item, Func<TItem, bool> match, bool saveToFile = true)
    {
        var existing = DataObject.Items.FirstOrDefault(match);
        if (existing != null) DataObject.Items.Remove(existing);
        DataObject.Items.Add(item);
        DataObject.LastUpdated = DateTime.Now;
        if(saveToFile) StorageService<T>.Service.StoreObject(DataObject);
    }
    public virtual bool Remove(Func<TItem, bool> match, bool saveToFile = true)
    {
        var existing = DataObject.Items.FirstOrDefault(match);
        if (existing == null) return false;

        DataObject.Items.Remove(existing);
        DataObject.LastUpdated = DateTime.Now;
        StorageService<T>.Service.StoreObject(DataObject);
        return true;
    }
    public virtual void Save() => StorageService<T>.Service.StoreObject(DataObject);
}