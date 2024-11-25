using Pulumi;
using Pulumi.AzureNative.Resources;
using Deployment = Pulumi.Deployment;

namespace PulumiWPC24;
public class WPC24Stack : Stack
{
    public WPC24Stack()
    {
        const string projectName = "pulumi-wpc24";
        var stackName = Deployment.Instance.StackName;
        var azureConfig = new Config("azure-native");
        
        #region Resource Group

        var resourceGroupName = $"{projectName}-{stackName}-rg";
        var resourceGroup = new ResourceGroup(resourceGroupName, new ResourceGroupArgs
        {
            ResourceGroupName = resourceGroupName
        });

        #endregion
    }
}