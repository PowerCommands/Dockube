using PainKiller.CommandPrompt.CoreLib.Core.BaseClasses;
using PainKiller.CommandPrompt.CoreLib.Core.Enums;
using PainKiller.CommandPrompt.CoreLib.Core.Events;
using PainKiller.CommandPrompt.CoreLib.Core.Extensions;
using PainKiller.CommandPrompt.CoreLib.Metadata.Attributes;
using PainKiller.CommandPrompt.CoreLib.Modules.StorageModule.DomainObjects;
using PainKiller.CommandPrompt.CoreLib.Modules.StorageModule.Services;

namespace PainKiller.CommandPrompt.CoreLib.Modules.StorageModule.Commands;

[CommandDesign(description:"Command displays your stored data files",
                  examples: ["//Show all storded data files","storage"])]
public class StorageCommand(string identifier) : ConsoleCommandBase<ApplicationConfiguration>(identifier)
{
    public override void OnInitialized()
    {
        var dir = StorageService<Customer>.Service.GetRootDirectory();
        var backupDir = StorageService<Customer>.Service.GetBackupDirectory();
        if (dir.Exists && backupDir.Exists) return;
        InitDirectories(dir.FullName, backupDir.FullName);
    }
    private void InitDirectories(string rootDir, string backupDir)
    {
        Directory.CreateDirectory(rootDir);
        Directory.CreateDirectory(backupDir);
    }
    public override RunResult Run(ICommandLineInput input)
    {
        var dir = StorageService<Customer>.Service.GetRootDirectory();
        Writer.WriteHeadLine($"{Emo.Directory.Icon()} App directory {dir.FullName} {dir.GetDirectorySize().GetDisplayFormattedFileSize()}");
        foreach (var file in dir.GetFiles()) Writer.WriteLine($"├──{Emo.File.Icon()} {file.Name}");
        Environment.CurrentDirectory = dir.FullName;
        EventBusService.Service.Publish(new WorkingDirectoryChangedEventArgs(Environment.CurrentDirectory));

        //Simple way of adding items
        var customers = new Customers{Items = [new Customer(), new Customer{Name = "Johnny Logan"}] };
        StorageService<Customers>.Service.StoreObject(customers);

        //More "advanced way" with Crud operations
        var customerStorage = new ObjectStorage<Customers, Customer>();
        var customer = new Customer { Name = "Helena Thompson" };
        customerStorage.Insert(customer, c => c.Id == customer.Id);

        var storedCustomers = customerStorage.GetItems();
        Writer.WriteTable(storedCustomers);
        return Ok();
    }
}