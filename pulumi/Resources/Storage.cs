using System.Collections.Generic;
using Pulumi;
using ST = Pulumi.AzureNative.Storage;

namespace PulumiWPC24.Resources;

public class StorageArgs : ResourceArgs
{
    public string Project { get; set; }
    public string Environment { get; set; }
    public string Location { get; set; }
    public Input<string> ResourceGroupName { get; set; }
}

public class Storage : ComponentResource
{
    [Output] public Output<string> StorageName { get; private set; }
    [Output] public Output<string> StorageId { get; private set; }
    
    readonly Dictionary<string, ST.Inputs.SkuArgs> skuArgs = new()
    {
        { "dev", new ST.Inputs.SkuArgs { Name = "Standard_LRS" } },
        { "qa", new ST.Inputs.SkuArgs { Name = "Standard_ZRS" } },
        { "prod", new ST.Inputs.SkuArgs { Name = "Standard_GRS" } }
    };
    
    public Storage(string name, StorageArgs args, ComponentResourceOptions options = null) : base("wpc:custom:storage", name, args, options)
    {
        var storageName = $"{args.Project}{args.Environment}st";
        var storageAccount = new ST.StorageAccount(storageName, new ST.StorageAccountArgs
        {
            AccountName = storageName,
            ResourceGroupName = args.ResourceGroupName,
            Sku = skuArgs[args.Environment],
            Kind = ST.Kind.StorageV2
        }, new CustomResourceOptions { Parent = this });

        var testContainer = new ST.BlobContainer("test", new ST.BlobContainerArgs
        {
            ResourceGroupName = args.ResourceGroupName,
            AccountName = storageAccount.Name,
            ContainerName = "test"
        }, new CustomResourceOptions { Parent = this });
        
        StorageName = storageAccount.Name;
        StorageId = storageAccount.Id;
        
        RegisterOutputs();
    }
}