using Pulumi;
using Pulumi.AzureNative.Insights;
using Pulumi.AzureNative.OperationalInsights;
using Pulumi.AzureNative.OperationalInsights.Inputs;

namespace PulumiWPC24.Resources;

public class LogArgs : ResourceArgs
{
    public string Project { get; set; }
    public string Environment { get; set; }
    public string Location { get; set; }
    public Input<string> ResourceGroupName { get; set; }
    public bool CreateNewLogAnalytics { get; set; } = true;
    public string ExistingLogAnalyticsName { get; set; }
    public bool CreateNewAppInsights { get; set; } = true;
}

public class Log : ComponentResource
{
    [Output] public Output<string> WorkspaceId { get; private set; }
    [Output] public Output<string> InstrumentationKey { get; private set; }
    
    public Log(string name, LogArgs args, ComponentResourceOptions options = null) : base("wpc:custom:log", name, args, options)
    {
        Workspace logWorkspace = null;
        if(args.CreateNewLogAnalytics)
        {
            var logWorkspaceName = $"{args.Project}-{args.Environment}-log";
            logWorkspace = new Workspace(logWorkspaceName, new WorkspaceArgs
            {
                WorkspaceName = logWorkspaceName,
                ResourceGroupName = args.ResourceGroupName,
                Sku = new WorkspaceSkuArgs { Name = "PerGB2018" },
                RetentionInDays = 30
            }, new CustomResourceOptions { Parent = this });
        }
        
        /*
        var logWorkspaceSharedKeys = Output.Tuple(args.ResourceGroupName, logWorkspace.Name).Apply(items =>
            GetSharedKeys.InvokeAsync(new GetSharedKeysArgs
            {
                ResourceGroupName = items.Item1,
                WorkspaceName = items.Item2
            }));
        */
        
        var getWorkspaceResult = GetWorkspace.Invoke(new GetWorkspaceInvokeArgs
        {
            ResourceGroupName = args.ResourceGroupName,
            WorkspaceName = args.CreateNewLogAnalytics ? logWorkspace!.Name : args.ExistingLogAnalyticsName
        });
        
        Component ai = null;
        if(args.CreateNewAppInsights)
        {
            var aiName = $"{args.Project}-{args.Environment}-ai";
            ai = new Component(aiName, new ComponentArgs
            {
                ApplicationType = ApplicationType.Web,
                FlowType = FlowType.Bluefield,
                Kind = "web",
                Location = args.Location,
                RequestSource = RequestSource.Rest,
                ResourceGroupName = args.ResourceGroupName,
                ResourceName = aiName,
                WorkspaceResourceId = getWorkspaceResult.Apply(s => s.Id)
            }, new CustomResourceOptions { Parent = this });
        }

        WorkspaceId = getWorkspaceResult.Apply(s => s.Id);
        InstrumentationKey = ai?.InstrumentationKey;
        
        RegisterOutputs();
    }
}