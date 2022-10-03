using System;
using System.Collections.Generic;
using Sharpliner.AzureDevOps.ConditionedExpressions;

namespace Sharpliner.GitHubActions;

public record Job
{
    public Job(string id)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentNullException((nameof(id)));
        Id = id;
    }

    public string Id { get; }
    public string? Name { get; set; }
    public List<string> Needs { get; } = new();

    // TODO
    public Container? RunsOn { get; set; }

    public Strategy? Strategy { get; set; }

    /// <summary>
    /// Allows to set the permissions granted to the Github token that will be used with the job. This
    /// setting will override that from the workflow.
    /// </summary>
    public Permissions Permissions { get; init; } = new();

    /// <summary>
    /// A map of environment variables that are available to all steps of the jobs. When more than one variable
    /// with the same name is used, the latter one will be used.
    /// </summary>
    public Environment? Environment { get; init; }

    /// <summary>
    /// Provide a concurrency context to ensure that just one workflow is executed at a given time.
    /// </summary>
    public Concurrency? Concurrency { get; set; }

    /// <summary>
    /// Represent the outputs of the job that will be available for other jobs.
    /// </summary>
    public Dictionary<string, string> Outputs { get; } = new();

    /// <summary>
    /// A map of environment variables that are available to all steps of the jobs. When more than one variable
    /// with the same name is used, the latter one will be used.
    /// </summary>
    public Dictionary<string, string> Env { get; } = new();

    /// <summary>
    /// Provide the default settings to be used by all jobs in the workflow.
    /// </summary>
    public Defaults Defaults { get; init; } = new();

    /// <summary>
    /// Condition to be evaluated. If the condition does not pass the job is skipped.
    /// </summary>
    public Condition? If { get; init; }

    /// <summary>
    /// The list of steps to be executed by the job.
    /// </summary>
    public List<Step> Steps { get; init; } = new();

    /// <summary>
    /// The list of services to be used for the job.
    /// </summary>
    public List<Service> Services { get; init; } = new();
}
