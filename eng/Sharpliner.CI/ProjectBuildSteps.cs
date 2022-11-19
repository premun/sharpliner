using System.Collections.Generic;
using Sharpliner.AzureDevOps;
using Sharpliner.AzureDevOps.ConditionedExpressions;

namespace Sharpliner.CI;

class ProjectBuildSteps : StepLibrary
{
    public const string PackagePath = "artifacts/packages";
    private readonly string _project;

    public ProjectBuildSteps(string project)
    {
        _project = project;
    }

    public override List<Conditioned<Step>> Steps => new()
    {
        StepTemplate(Pipelines.TemplateLocation + "install-dotnet-sdk.yml", new()
        {
            { "version", "7.0.100" }
        }),

        Powershell
            .Inline("New-Item -Path 'artifacts' -Name 'packages' -ItemType 'directory'")
            .DisplayAs($"Create {PackagePath}"),

        DotNet
            .Build(_project, includeNuGetOrg: true)
            .DisplayAs("Build"),
    };
}
