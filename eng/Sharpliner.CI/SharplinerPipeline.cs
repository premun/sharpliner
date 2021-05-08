using Sharpliner.AzureDevOps;
using Sharpliner.AzureDevOps.Tasks;

namespace Sharpliner.CI
{
    public class SharplinerPipeline : AzureDevOpsPipelineDefinition
    {
        public override string TargetFile => "azure-pipelines.yml";

        public override TargetPathType TargetPathType => TargetPathType.RelativeToGitRoot;

        public override AzureDevOpsPipeline Pipeline => new()
        {
            Trigger = new DetailedTrigger
            {
                Batch = true,
                Branches = new()
                {
                    Include = { "main" }
                }
            },

            Pr = new BranchPrTrigger("main"),

            Stages =
            {
                new Stage("Build", "Build")
                {
                    Jobs =
                    {
                        new Job("Build", "Build and test")
                        {
                            Pool = new HostedPool("Azure Pipelines", "windows-latest"),
                            Steps =
                            {
                                new AzureDevOpsTask("UseDotNet@2", "Use .NET 5")
                                {
                                    Inputs = new TaskInputs
                                    {
                                        { "packageType", "sdk" },
                                        { "version", "5.0.102" },
                                    }
                                },

                                new AzureDevOpsTask("DotNetCoreCLI@2", "dotnet restore")
                                {
                                    Inputs = new TaskInputs
                                    {
                                        { "command", "restore" },
                                        { "includeNuGetOrg", true },
                                        { "projects", "Sharpliner.sln" },
                                    }
                                },

                                new AzureDevOpsTask("DotNetCoreCLI@2", "dotnet test")
                                {
                                    Inputs = new TaskInputs
                                    {
                                        { "command", "test" },
                                        { "includeNuGetOrg", true },
                                        { "projects", "Sharpliner.sln" },
                                    }
                                },
                            }
                        }
                    }
                },
            }
        };
    }
}
