using System.Collections.Generic;
using Pulumi;
using Pulumi.AzureNative.Sql;
using Pulumi.AzureNative.Sql.Inputs;

namespace PulumiWPC24.Resources;

public class SqlArgs : ResourceArgs
{
    public string Project { get; set; }
    public string Environment { get; set; }
    public string Location { get; set; }
    public Input<string> ResourceGroupName { get; set; }
    public bool CreateNewServer { get; set; } = true;
    public Input<string> ExistingServerName { get; set; }
    public Input<string> AdministratorManagedIdentityId { get; set; }
    public Input<string> AdministratorManagedIdentityName { get; set; }
    public string AllowedIpAddresses { get; set; }
    public bool AllowAzure { get; set; }
    public bool CreateNewDb { get; set; } = true;
}

public class Sql : ComponentResource
{
    readonly Dictionary<string, SkuArgs> skuArgs = new()
    {
        { "dev", new SkuArgs { Name = "S1"} },
        { "qa", new SkuArgs { Name = "S2" } },
        { "prod", new SkuArgs { Name = "S3"} }
    };
    
    public Sql(string name, SqlArgs args, ComponentResourceOptions options = null) : base("wpc:custom:sql", name, args, options)
    {
        var azureConfig = new Config("azure-native");

        Server sqlServer = null;
        if (args.CreateNewServer)
        {
            var sqlServerName = $"{args.Project}-{args.Environment}-sql";
            sqlServer = new Server(sqlServerName, new ServerArgs
            {
                Administrators = new ServerExternalAdministratorArgs
                {
                    PrincipalType = PrincipalType.Application,
                    AzureADOnlyAuthentication = true,
                    AdministratorType = "ActiveDirectory",
                    Login = args.AdministratorManagedIdentityName,
                    Sid = args.AdministratorManagedIdentityId,
                    TenantId = azureConfig.Require("tenantId")
                },
                Location = args.Location,
                PublicNetworkAccess = ServerNetworkAccessFlag.Enabled,
                ResourceGroupName = args.ResourceGroupName,
                ServerName = sqlServerName
            }, new CustomResourceOptions { Parent = this });
        }
        
        var getServerResult = GetServer.Invoke(new GetServerInvokeArgs
        {
            ResourceGroupName = args.ResourceGroupName,
            ServerName = args.CreateNewServer ? sqlServer!.Name : args.ExistingServerName
        });

        Database sqlDb = null;
        if (args.CreateNewDb)
        {
            var sqlDbName = $"{args.Project}-{args.Environment}-sqldb";
            sqlDb = new Database(sqlDbName, new DatabaseArgs
            {
                DatabaseName = sqlDbName,
                Location = args.Location,
                ResourceGroupName = args.ResourceGroupName,
                ServerName = getServerResult.Apply(s => s.Name),
                Sku = skuArgs[args.Environment]
            }, new CustomResourceOptions { Parent = this });
        }
        
        if (args.AllowAzure)
        {
            var azureRule = new FirewallRule("allow_Azure", new FirewallRuleArgs
            {
                StartIpAddress = "0.0.0.0",
                EndIpAddress = "255.255.255.255",
                FirewallRuleName = "allow_Azure",
                ResourceGroupName = args.ResourceGroupName,
                ServerName = getServerResult.Apply(s => s.Name)
            }, new CustomResourceOptions { Parent = this });
        }
        
        if(args.AllowedIpAddresses != null)
        {
            var ipAddresses = args.AllowedIpAddresses.Split(',');
            foreach (var ip in ipAddresses)
            {
                var ipRule = new FirewallRule($"allow_{ip}", new FirewallRuleArgs
                {
                    StartIpAddress = ip,
                    EndIpAddress = ip,
                    FirewallRuleName = $"allow_{ip}",
                    ResourceGroupName = args.ResourceGroupName,
                    ServerName = getServerResult.Apply(s => s.Name)
                }, new CustomResourceOptions { Parent = this });
            }
        }
        
        RegisterOutputs(new Dictionary<string, object>
        {
            { "SqlServerFqdn", getServerResult.Apply(s => s.FullyQualifiedDomainName) },
            { "SqlDbName", sqlDb?.Name }
        });
    }
}