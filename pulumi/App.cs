using System.Collections.Generic;
using Pulumi;
using Pulumi.AzureNative.Web;
using Pulumi.AzureNative.Web.Inputs;

namespace PulumiWPC24;

public class AppArgs : ResourceArgs
{
    public string Project { get; set; }
    public Input<string> Environment { get; set; }
    public Input<string> Location { get; set; }
    public Input<string> ResourceGroupName { get; set; }
    public bool CreateNewPlan { get; set; }
    public Input<string> ExistingPlanId { get; set; }
}

public class App : ComponentResource
{
    [Output(nameof(AppUri))]
    public Output<string> AppUri { get; private set; }
    [Output(nameof(AppName))]
    public Output<string> AppName { get; private set; }
    [Output(nameof(AppIdentity))]
    public Output<string> AppIdentity { get; private set; }

    readonly Dictionary<Input<string>, SkuDescriptionArgs> skuArgs = new()
    {
        { "dev", new SkuDescriptionArgs { Name = "B1", Capacity = 1 } },
        { "qa", new SkuDescriptionArgs { Name = "S1", Capacity = 2 } },
        { "prod", new SkuDescriptionArgs { Name = "P1v2", Capacity = 2 } }
    };

    public App(string name, AppArgs args, ComponentResourceOptions options = null) : base("custom:app", name, args, options)
    { 
        AppServicePlan plan = null;
        if(args.CreateNewPlan)
        {
            var planName = $"{args.Project}-{args.Environment}-plan";
            plan = new AppServicePlan(planName, new AppServicePlanArgs
            {
                Kind = "app",
                Location = args.Location,
                Name = planName,
                ResourceGroupName = args.ResourceGroupName,
                Sku = skuArgs[args.Environment]
            });
        }
        
        var appName = $"{args.Project}-{args.Environment}-app";
        var app = new WebApp(appName, new WebAppArgs
        {
            Kind = "app",
            Location = args.Location,
            Name = appName,
            ResourceGroupName = args.ResourceGroupName,
            ServerFarmId = args.CreateNewPlan ? plan!.Id : args.ExistingPlanId
        });
    }
}