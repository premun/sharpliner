using Sharpliner.GitHubActions;

namespace Sharpliner.Tests.GitHubActions;

class CodeQLAnalysisWorkflow : WorkflowDefinition
{
    public override string TargetFile => ".github/workflows/codeql-analysis.yml";

    public override TargetPathType TargetPathType => TargetPathType.RelativeToGitRoot;

    public override Workflow Workflow => new()
    {
        Name = "CodeQL",

        On = new()
        {
            Push = new()
            {
                Branches = { "main" },
            },
            PullRequest = new()
            {
                Branches = { "main" },
            },
        },

        Jobs =
        {
            new Job("Analyze")
            {
                RunsOn = "ubuntu-latest",

                Steps =
                {
                    new()
                    {

                    },

                    new Step("actions/setup-dotnet@v1")
                    {
                        With = new()
                        {
                            dotnetVersion = "5.0.x",
                        },
                    },

                    new Step("github/codeql-action/init@v1")
                    {
                        With = new()
                        {
                            languages = "csharp",
                        },
                    },

                    new Step("github/codeql-action/autobuild@v1"),

                    new Step("github/codeql-action/analyze@v1"),
                },
            },
        }
    };
}
