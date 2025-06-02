namespace PainKiller.CommandPrompt.CoreLib.Modules.StorageModule.DomainObjects;

public class Customer
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = "James Tromb";
}