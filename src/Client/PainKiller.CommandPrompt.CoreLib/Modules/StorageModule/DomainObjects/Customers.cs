using PainKiller.CommandPrompt.CoreLib.Modules.StorageModule.Contracts;

namespace PainKiller.CommandPrompt.CoreLib.Modules.StorageModule.DomainObjects;

public class Customers : IDataObjects<Customer>
{
    public DateTime LastUpdated { get; set; }
    public List<Customer> Items { get; set; } = [];
}