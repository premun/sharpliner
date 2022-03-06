using Sharpliner.AzureDevOps;

namespace Sharpliner.Tests.AzureDevOps;

/// <summary>
/// This shows an example pipeline with all possible fields set. You can use it as a reference.
/// </summary>
class ShowcasePipeline : PipelineDefinition
{
    public override string TargetFile => "ci/pipeline.yml";
    public override TargetPathType TargetPathType => TargetPathType.RelativeToGitRoot;

    public override string[]? Header { get; } = new[]
    {
        "You can customize the header which will appear in the resulting YAML file",
        "Each array item goes on a separate line",
    };

    public override Pipeline Pipeline => new()
    {
        // https://docs.microsoft.com/en-us/azure/devops/pipelines/process/run-number?view=azure-devops&tabs=yaml
        Name = "Showcase.Pipeline $(Date:yyyMMdd).$(Rev:rr)",

        Pr = new PrTrigger("main", "production")
        {
            AutoCancel = true,
            Drafts = true,
            Paths = new()
            {
                // When initializaing ConditionedList, you can use this notation (requires C# 10):
                Include = { "**/*.cs" },
                Exclude = { "docs/**/*", "artifacts/**/*" },
            }
        },

        Schedules =
        {
            new("* * * * 30 *", "main")
        },

        // When initializaing (Conditioned)List, you can use this notation (requires C# 10):
        Variables =
        {
            // More on if/else statements here: https://github.com/sharpliner/sharpliner/blob/main/docs/AzureDevOps/DefinitionReference.md#conditioned-expressions
            If.And(IsPullRequest, IsNotBranch("main"))
                .Variable("isPr", true)
                .Group("pr-keyvault")
            .Else
                .Group("ci-keyvault"),
        },
    };
}
