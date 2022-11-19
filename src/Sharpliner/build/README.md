Sharpliner is a .NET library that lets you use C# for Azure DevOps pipeline definition instead of YAML.
Exchange YAML indentation problems for the type-safe environment of C# and let the intellisense speed up your work!

## Getting started

All you have to do is reference our [NuGet package](https://www.nuget.org/packages/Sharpliner/)  in your project, override a class with your definition and `dotnet build` the project! Dead simple!

For more detailed steps, check our [documentation](https://github.com/sharpliner/sharpliner/blob/main/docs/AzureDevOps/GettingStarted.md).

## Example

```csharp
// Just override prepared abstract classes and `dotnet build` the project, nothing else is needed!
// For a full list of classes you can override
//    see https://github.com/sharpliner/sharpliner/blob/main/src/Sharpliner/AzureDevOps/PublicDefinitions.cs
// You can also generate collections of definitions dynamically
//    see https://github.com/sharpliner/sharpliner/blob/main/docs/AzureDevOps/DefinitionCollections.md
class PullRequestPipeline : SingleStagePipelineDefinition
{
    // Say where to publish the YAML to
    public override string TargetFile => "eng/pr.yml";
    public override TargetPathType TargetPathType => TargetPathType.RelativeToGitRoot;

    public override SingleStagePipeline Pipeline => new()
    {
        Pr = new PrTrigger("main"),

        Variables =
        {
            // YAML ${{ if }} conditions are available with handy macros that expand into the
            // expressions such as comparing branch names. We also have "else"
            If.IsBranch("net-6.0")
                .Variable("DotnetVersion", "6.0.100")
                .Group("net6-keyvault")
            .Else
                .Variable("DotnetVersion", "5.0.202"),
        },

        Jobs =
        {
            new Job("Build")
            {
                Pool = new HostedPool("Azure Pipelines", "windows-latest"),
                Steps =
                {
                    // Many tasks have helper methods for shorter notation
                    DotNet.Install.Sdk(variables["DotnetVersion"]),

                    // You can also specify any pipeline task in full too
                    Task("DotNetCoreCLI@2", "Build and test") with
                    {
                        Inputs = new()
                        {
                            { "command", "test" },
                            { "projects", "src/MyProject.sln" },
                        }
                    },

                    // Frequently used ${{ if }} statements have readable macros
                    If.IsPullRequest
                        // You can load script contents from a .ps1 file and inline them into YAML
                        // This way you can write scripts with syntax highlighting separately
                        .Step(Powershell.FromResourceFile("New-Report.ps1", "Create build report")),
                }
            }
        },
    };
}
```

## Sharpliner features

Apart from the obvious benefits of using static type language with IDE support, not having to have to deal with indentation problems ever again, being able to split the code easily or the ability to generate YAML programatically, there are several other benefits of using Sharpliner.

### Intellisense
One of the best things when using Sharpliner is that you won't have to go the YAML reference every time you're adding a new piece of your pipeline.
Having everything strongly typed will make your IDE give you hints all the way!

![Example intellisense for pipeline variables](https://raw.githubusercontent.com/sharpliner/sharpliner/main/docs/images/variables-intellisense.png)

### Nice APIs

Imagine you want to install the .NET SDK. For that, Azure Pipelines have the `DotNetCoreCLI@2` task.
However, this task's specification is quite long since the task does many things:

```yaml
# .NET Core
# Build, test, package, or publish a dotnet application, or run a custom dotnet command
# https://docs.microsoft.com/en-us/azure/devops/pipelines/tasks/build/dotnet-core-cli?view=azure-devops
- task: DotNetCoreCLI@2
  inputs:
    command: 'build' # Options: build, push, pack, publish, restore, run, test, custom
    publishWebProjects: true # Required when command == Publish
    projects: # Optional
    custom: # Required when command == Custom
    arguments: # Optional
    publishTestResults: true # Optional
    testRunTitle: # Optional
    zipAfterPublish: true # Optional
    versioningScheme: 'off' # Options: off, byPrereleaseNumber, byEnvVar, byBuildNumber
    versionEnvVar: # Required when versioningScheme == byEnvVar
    majorVersion: '1' # Required when versioningScheme == ByPrereleaseNumber
    minorVersion: '0' # Required when versioningScheme == ByPrereleaseNumber
    patchVersion: '0' # Required when versioningScheme == ByPrereleaseNumber
    buildProperties: # Optional
    verbosityPack: 'Detailed' # Options: -, quiet, minimal, normal, detailed, diagnostic
    workingDirectory:
```

Notice how some of the properties are only valid in a specific combination with other.
With Sharpliner, we remove some of this complexity using nice fluent APIs:

```csharp
DotNet.Install.Sdk(parameters["version"]),

DotNet.Restore.FromFeed("dotnet-7-preview-feed", includeNuGetOrg: false) with
{
    ExternalFeedCredentials = "feeds/dotnet-7",
    NoCache = true,
    RestoreDirectory = ".packages",
},

DotNet.Build("src/MyProject.csproj") with
{
    Timeout = TimeSpan.FromMinutes(20)
},
```

### Useful macros
Some very common pipeline patterns such as comparing the current branch name or detecting pull requests are very cumbersome to do in YAML (long conditions full of complicated `${{ if }}` syntax).
For many of these, we have handy macros so that you get more readable and shorter code.

For example this YAML

```yaml
${{ if eq(variables['Build.SourceBranch'], 'refs/heads/production') }}:
    name: rg-suffix
    value: -pr
${{ if ne(variables['Build.SourceBranch'], 'refs/heads/production') }}:
    name: rg-suffix
    value: -prod
```

can become this C#

```csharp
If.IsBranch("production")
    .Variable("rg-suffix", "-pr")
.Else
    .Variable("rg-suffix", "-prod")
```

### Re-usable pipeline blocks
Sharpliner lets you re-use code more easily than YAML templates do.
Apart from obvious C# code re-use, you can also define sets of C# building blocks and re-use them in your pipelines:

```csharp
class ProjectBuildSteps : StepLibrary
{
    public override List<Conditioned<Step>> Steps => new()
    {
        DotNet.Install.Sdk("6.0.100"),

        If.IsBranch("main")
            .Step(DotNet.Restore.Projects("src/MyProject.sln")),

        DotNet.Build("src/MyProject.sln"),
    };
}
```

You can then reference this library in between build steps and it will get expanded into the pipeline's YAML:

```csharp
...
    new Job("Build")
    {
        Steps =
        {
            Script.Inline("echo 'Hello World'"),

            StepLibrary<ProjectBuildSteps>(),

            Script.Inline("echo 'Goodbye World'"),
        }
    }
...
```

More about this feature can be found [here (DefinitionLibraries.md)](https://github.com/sharpliner/sharpliner/blob/main/docs/AzureDevOps/DefinitionLibraries.md).


### Sourcing scripts from files
When you need to add cmd, PowerShell or bash steps into your pipeline, mainatining these bits inside YAML can be error prone.
With Sharpliner you can keep scripts in their own files (`.ps1`, `.sh`..) where you get the natural environment you're used to such as syntax highlighting.
Sharpliner gives you APIs to load these on build time and include them inline:

```csharp
Steps =
{
    Bash.FromResourceFile("embedded-script.sh") with
    {
        DisplayName = "Run post-build clean-up",
        Timeout = TimeSpan.FromMinutes(5),
    }
}
```

### Correct variable/parameter types
Frequent struggle people have with Azure pipelines is using the [right type of variable](https://docs.microsoft.com/en-us/azure/devops/pipelines/process/variables?view=azure-devops&tabs=yaml%2Cbatch#understand-variable-syntax) in the right context.
Be it a `${{ compile time parameter }}`, a `variable['used in runtime']` or a `$(macro)` syntax, with Sharpliner you won't have to worry about which one to pick as it understands the context and selects the right one for you.

### Pipeline validation
Your pipeline definition can be validated during publishing and you can uncover issues, such as typos inside `dependsOn`, you would only find by trying to run the pipeline in CI.
This gives you a faster dev loop and greater productivity.

We are continuosly adding new validations as we find new error-prone spots.
Each validation can be individually configured/silenced in case you don't wish to take advantage of these:

```csharp
class YourCustomConfiguration : SharplinerConfiguration
{
    public override void Configure()
    {
        // You can set severity for various validations
        Validations.DependsOn = ValidationSeverity.Off;
        Validations.Name = ValidationSeverity.Warning;

        // You can also further customize serialization
        Serialization.PrettifyYaml = false;
        Serialization.UseElseExpression = true;
        Serialization.IncludeHeaders = false;

        // You can add hooks that execute during the publish process
        Hooks.BeforePublish = (definition, path) => {};
        Hooks.AfterPublish = (definition, path) => {};
    }
}
```
