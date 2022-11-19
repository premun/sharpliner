using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Sharpliner.AzureDevOps.ConditionedExpressions;
using YamlDotNet.Serialization;

namespace Sharpliner.AzureDevOps;

public abstract record JobBase : IDependsOn
{
    private Conditioned<Pool>? _pool;
    private Conditioned<ContainerReference>? _container;

    [YamlIgnore]
    public string Name { get; init; }

    /// <summary>
    /// Friendly name to display in the UI
    /// </summary>
    [YamlMember(Order = 100)]
    [DisallowNull]
    public string? DisplayName { get; init; }

    /// <summary>
    /// List of names of other jobs this job depends on
    /// </summary>
    [YamlMember(Order = 200)]
    public ConditionedList<string> DependsOn { get; init; } = new();

    /// <summary>
    /// Specifies which pool to use for a job of the pipeline
    /// A pool specification also holds information about the job's strategy for running.
    /// </summary>
    [YamlMember(Order = 300)]
    public Conditioned<Pool>? Pool { get => _pool; init => _pool = value?.GetRoot(); }

    /// <summary>
    /// Container to run this job inside of.
    /// </summary>
    [YamlMember(Order = 500)]
    [DisallowNull]
    public Conditioned<ContainerReference>? Container { get => _container; init => _container = value?.GetRoot(); }

    /// <summary>
    /// Specifies variables at the job level
    /// You can add hard-coded values directly, reference variable groups, or insert via variable templates.
    /// </summary>
    [YamlMember(Order = 600)]
    public ConditionedList<VariableBase> Variables { get; init; } = new();

    /// <summary>
    /// How long to run the job before automatically cancelling
    /// </summary>
    [YamlIgnore]
    [DisallowNull]
    public TimeSpan? Timeout { get; init; }

    [YamlMember(Order = 800)]
    public int? TimeoutInMinutes => Timeout != null ? (int)Timeout.Value.TotalMinutes : null;

    /// <summary>
    /// How much time to give 'run always even if cancelled tasks' before killing them
    /// </summary>
    [YamlIgnore]
    [DisallowNull]
    public TimeSpan? CancelTimeout { get; init; }

    [YamlMember(Order = 900)]
    public int? CancelTimeoutInMinutes => CancelTimeout != null ? (int)CancelTimeout.Value.TotalMinutes : null;

    /// <summary>
    /// What to clean up before the job runs
    /// </summary>
    [YamlMember(Order = 1000)]
    [DefaultValue(JobWorkspace.Outputs)]
    public JobWorkspace Workspace { get; init; } = JobWorkspace.Outputs;

    /// <summary>
    /// Container resources to run as a service container.
    /// </summary>
    [YamlMember(Order = 1100)]
    public Dictionary<string, string> Services { get; init; } = new();

    [YamlMember(Order = 1100)]
    [DisallowNull]
    public InlineCondition? Condition { get; init; }

    /// <summary>
    /// 'true' if future jobs should run even if this job fails
    /// defaults to 'false'
    /// </summary>
    [YamlMember(Order = 1200)]
    public bool ContinueOnError { get; init; } = false;

    protected JobBase(string name, string? displayName = null)
    {
        Pipeline.ValidateName(name);

        Name = name ?? throw new ArgumentNullException(nameof(name));

        if (displayName != null)
        {
            DisplayName = displayName;
        }
    }
}
