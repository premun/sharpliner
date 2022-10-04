using System.Collections.Generic;

namespace Sharpliner.GitHubActions;

/// <summary>
/// A workflow represents and automatic process in GitHub that have one more more steps.
/// </summary>
public record Workflow
{
    /// <summary>
    /// The name of your workflow. GitHub displays the names of your workflows on your repository's "Actions" tab.
    /// If you omit name, GitHub sets it to the workflow file path relative to the root of the repository.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// The name for workflow runs generated from the workflow.
    /// GitHub displays the workflow run name in the list of workflow runs on your repository's "Actions" tab.
    /// If you omit run-name, the run name is set to event-specific information for the workflow run.
    ///
    /// For example, for a workflow triggered by a push or pull_request event, it is set as the commit message.
    ///
    /// This value can include expressions and can reference the github and inputs contexts.
    ///
    /// Example:
    /// run-name: Deploy to ${{ inputs.deploy_target }} by @${{ github.actor }}
    /// </summary>
    public string? RunName { get; set; }

    /// <summary>
    /// To automatically trigger a workflow, use on to define which events can cause the workflow to run. 
    ///
    /// You can define single or multiple events that can a trigger workflow, or set a time schedule.
    /// You can also restrict the execution of a workflow to only occur for specific files, tags, or branch changes.
    /// </summary>
    public Trigger On { get; set; } = new();

    /// <summary>
    /// Allows to set the permissions granted to the Github token that will be used with the workflow. This
    /// setting will apply to all the jobs in a workflow. You can override this setting per job.
    /// </summary>
    public Permissions Permissions { get; set; } = new();

    /// <summary>
    /// A map of environment variables that are available to the steps of all jobs. When more than one variable
    /// with the same name is used, the latter one will be used.
    /// </summary>
    public Dictionary<string, string> Env { get; set; } = new();

    /// <summary>
    /// Provide a concurrency context to ensure that just one workflow is executed at a given time.
    /// </summary>
    public Concurrency? Concurrency { get; set; }

    /// <summary>
    /// Provide the default settings to be used by all jobs in the workflow.
    /// </summary>
    public Defaults Defaults { get; set; } = new();

    /// <summary>
    /// List of jobs to be executed by the workflow.
    /// </summary>
    public List<Job> Jobs { get; set; } = new();
}
