using System.Diagnostics.CodeAnalysis;
using Sharpliner.AzureDevOps.ConditionedExpressions;
using YamlDotNet.Serialization;

namespace Sharpliner.AzureDevOps;

public record Stage : IDependsOn
{
    /// <summary>
    /// Name of the stage (A-Z, a-z, 0-9, and underscore).
    /// </summary>
    [YamlMember(Alias = "stage", Order = 1)]
    public string Name { get; }

    /// <summary>
    /// Friendly name to display in the UI
    /// </summary>
    [YamlMember(Order = 2)]
    [DisallowNull]
    public string? DisplayName { get; init; }

    /// <summary>
    /// List of names of other jobs this job depends on
    /// </summary>
    [YamlMember(Order = 100)]
    public ConditionedList<string> DependsOn { get; init; } = new();

    /// <summary>
    /// Specifies variables at the job level
    /// You can add hard-coded values directly, reference variable groups, or insert via variable templates.
    /// </summary>
    [YamlMember(Order = 200)]
    public ConditionedList<VariableBase> Variables { get; init; } = new();

    /// <summary>
    /// A job is a collection of steps run by an agent or on a server.
    /// Jobs can run conditionally and might depend on earlier jobs.
    /// More details can be found in <see href="https://docs.microsoft.com/en-us/azure/devops/pipelines/process/phases?tabs=yaml&amp;view=azure-devops">official Azure DevOps pipelines documentation</see>.
    /// </summary>
    [YamlMember(Order = 300)]
    public ConditionedList<JobBase> Jobs { get; init; } = new();

    [YamlMember(Order = 400)]
    [DisallowNull]
    public InlineCondition? Condition { get; init; }

    public Stage(string name, string? displayName = null)
    {
        Name = name ?? throw new System.ArgumentNullException(nameof(name));

        if (displayName != null)
        {
            DisplayName = displayName;
        }
    }
}
