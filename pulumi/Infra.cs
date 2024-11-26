using Pulumi;
using Pulumi.AzureNative.Authorization;
using Pulumi.AzureNative.Resources;
using Deployment = Pulumi.Deployment;

namespace PulumiWPC24;
public class Infra : Stack
{
    public Infra()
    {
        const string projectName = "pulumi-wpc24";
        var stackName = Deployment.Instance.StackName;
        var azureConfig = new Config("azure-native");
        var location = azureConfig.Require("location");
        
        #region Resource Group

        var resourceGroupName = $"{projectName}-{stackName}-rg";
        var resourceGroup = new ResourceGroup(resourceGroupName, new ResourceGroupArgs
        {
            ResourceGroupName = resourceGroupName
        });

        #endregion
        
        #region Custom Resources
        
        const string logName = "wpc-custom-log";
        var log = new Resources.Log(logName, new Resources.LogArgs
        {
            Project = projectName,
            Environment = stackName,
            Location = location,
            ResourceGroupName = resourceGroup.Name
        });
        
        const string appName = "wpc-custom-app";
        var app = new Resources.App(appName, new Resources.AppArgs
        {
            Project = projectName,
            Environment = stackName,
            Location = location,
            ResourceGroupName = resourceGroup.Name,
            CreateNewPlan = true
        });
        
        const string sqlName = "wpc-custom-sql";
        var sql = new Resources.Sql(sqlName, new Resources.SqlArgs
        {
            Project = projectName,
            Environment = stackName,
            Location = location,
            ResourceGroupName = resourceGroup.Name,
            CreateNewServer = true,
            AdministratorManagedIdentityName = app.AppName,
            AdministratorManagedIdentityId = app.AppIdentity
        });
        
        const string storageName = "wpc-custom-storage";
        var storage = new Resources.Storage(storageName, new Resources.StorageArgs
        {
            Project = projectName,
            Environment = stackName,
            Location = location,
            ResourceGroupName = resourceGroup.Name
        });
        
        #endregion 
        
        #region Roles
        
        const string storageBlobDataContributor = "ba92f5b4-2d11-453d-a403-e96b0029c9fe";

        var apiTableStorageDataContributorRoleName = $"{projectName}-{stackName}-app-sbdc-role";
        var apiTableStorageDataContributorRole = new RoleAssignment(apiTableStorageDataContributorRoleName, new RoleAssignmentArgs
        {
            PrincipalId = app.AppIdentity,
            PrincipalType = PrincipalType.ServicePrincipal,
            RoleAssignmentName = "1aa74c01-e432-4fbc-9c73-b37d19056aa5",
            RoleDefinitionId = $"/providers/Microsoft.Authorization/roleDefinitions/{storageBlobDataContributor}",
            Scope = storage.StorageId
        });
        
        #endregion
    }
}