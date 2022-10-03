using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sharpliner.GitHubActions;

/// <summary>
/// Base abstract class for all webhook related triggers.
/// </summary>
public abstract record Webhook : Event;

// All the following enum types look verbose.. but we are
// looking into the future with https://github.com/dotnet/csharplang/issues/2926
// ideally, all these will look as nice as:
//
// var cr = new CheckRun {
//     Activities = { Created, Requested }
// }
//
// we will have to wait until c# does implement the feature, so for the time
// being we will do:
//
// var cr = new CheckRun {
//     Activities = { CheckRun.Activity.Created, CheckRun.Activity.Requested }
// }
//
// verbose, but type safe and autocomplete will help. If not, get a better editor!

/// <summary>
/// Triggers the workflow for the 'check_run' event. For than one activity can trigger the
/// event. By default all activities trigger the workflow. Use the Activities property to filter them.
/// </summary>
public record CheckRun : Webhook
{
    public enum Activity
    {
        Created,
        Rerequested,
        Completed,
        RequestedAction,
    }

    /// <summary>
    /// List of activities that will be considered when the event is triggered.
    /// </summary>
    public List<Activity> Activities { get; } = new();
}

/// <summary>
/// Triggers the workflow for the 'check_suite' event. For than one activity can trigger the
/// event. By default all activities trigger the workflow. Use the Activities property to filter them.
/// </summary>
public record CheckSuite : Webhook
{
    public enum Activity
    {
        Completed,
        Requested,
        Rerequested,
    }

    /// <summary>
    /// List of activities that will be considered when the event is triggered.
    /// </summary>
    public List<Activity> Activities { get; } = new();
}

/// <summary>
/// Triggers the workflow for the 'create' event.
/// </summary>
public record Create : Webhook;

/// <summary>
/// Triggers the workflow for the 'delete' event.
/// </summary>
public record Delete : Webhook;

/// <summary>
/// Triggers the workflow for the 'deployment' event.
/// </summary>
public record Deployment : Webhook;

/// <summary>
/// Triggers the workflow for the 'deployment_status' event.
/// </summary>
public record DeploymentStatus : Webhook;

/// <summary>
/// Triggers the workflow for the 'fork' event.
/// </summary>
public record Fork : Webhook;

/// <summary>
/// Triggers the workflow for the 'gollum' event.
/// </summary>
public record Gollum : Webhook;

/// <summary>
/// Triggers the workflow for the 'issue_comment' event. For than one activity can trigger the
/// event. By default all activities trigger the workflow. Use the Activities property to filter them.
/// </summary>
public record IssueComment : Webhook
{
    public enum Activity
    {
        Created,
        Edited,
        Deleted,
    }

    /// <summary>
    /// List of activities that will be considered when the event is triggered.
    /// </summary>
    public List<Activity> Activities { get; } = new();
}

/// <summary>
/// Triggers the workflow for the 'issues' event. For than one activity can trigger the
/// event. By default all activities trigger the workflow. Use the Activities property to filter them.
/// </summary>
public record Issues : Webhook
{
    public enum Activity
    {
        Opened,
        Edited,
        Deleted,
        Transferred,
        Pinned,
        Unpinned,
        Closed,
        Reopened,
        Assigned,
        Unassigned,
        Labeled,
        Unlabeled,
        Locked,
        Unlocked,
        Milestoned,
        Demilestoned,
    }

    /// <summary>
    /// List of activities that will be considered when the event is triggered.
    /// </summary>
    public List<Activity> Activities { get; } = new();
}

/// <summary>
/// Triggers the workflow for the 'label' event. For than one activity can trigger the
/// event. By default all activities trigger the workflow. Use the Activities property to filter them.
/// </summary>
public record Label : Webhook
{
    public enum Activity
    {
        Created,
        Edited,
        Deleted,
    }

    /// <summary>
    /// List of activities that will be considered when the event is triggered.
    /// </summary>
    public List<Activity> Activities { get; } = new();
}

/// <summary>
/// Triggers the workflow for the 'milestone' event. For than one activity can trigger the
/// event. By default all activities trigger the workflow. Use the Activities property to filter them.
/// </summary>
public record Milestone : Webhook
{
    public enum Activity
    {
        Created,
        Closed,
        Opened,
        Edited,
        Deleted,
    }

    /// <summary>
    /// List of activities that will be considered when the event is triggered.
    /// </summary>
    public List<Activity> Activities { get; } = new();
}

/// <summary>
/// Triggers the workflow for the 'page_build' event.
/// </summary>
public record PageBuild : Webhook { }

/// <summary>
/// Triggers the workflow for the 'project' event. For than one activity can trigger the
/// event. By default all activities trigger the workflow. Use the Activities property to filter them.
/// </summary>
public record Project : Webhook
{
    public enum Activity
    {
        Created,
        Updated,
        Closed,
        Reopened,
        Edited,
        Deleted,
    }

    /// <summary>
    /// List of activities that will be considered when the event is triggered.
    /// </summary>
    public List<Activity> Activities { get; } = new();
}

/// <summary>
/// Triggers the workflow for the 'project_card' event. For than one activity can trigger the
/// event. By default all activities trigger the workflow. Use the Activities property to filter them.
/// </summary>
public record ProjectCard : Webhook
{
    public enum Activity
    {
        Created,
        Moved,
        ConvertedToAnIssue,
        Edited,
        Deleted,
    }

    /// <summary>
    /// List of activities that will be considered when the event is triggered.
    /// </summary>
    public List<Activity> Activities { get; } = new();
}

/// <summary>
/// Triggers the workflow for the 'project_column' event. For than one activity can trigger the
/// event. By default all activities trigger the workflow. Use the Activities property to filter them.
/// </summary>
public record ProjectColumn : Webhook
{
    public enum Activity
    {
        Created,
        Updated,
        Moved,
        Deleted,
    }

    /// <summary>
    /// List of activities that will be considered when the event is triggered.
    /// </summary>
    public List<Activity> Activities { get; } = new();
}
public record Public : Webhook { }

/// <summary>
/// Triggers the workflow for the 'pull_request' event. For than one activity can trigger the
/// event. By default all activities trigger the workflow. Use the Activities property to filter them.
/// </summary>
public record PullRequest : Webhook
{
    public enum Activity
    {
        Assigned,
        Unassigned,
        Labeled,
        Unlabeled,
        Opened,
        Edited,
        Closed,
        Reopened,
        Synchronize,
        ReadyForReview,
        Locked,
        Unlocked,
        ReviewRequested,
        ReviewRequestRemoved,
    }

    /// <summary>
    /// List of activities that will be considered when the event is triggered.
    /// </summary>
    public List<Activity> Activities { get; } = new();
}

/// <summary>
/// Triggers the workflow for the 'pull_request_review' event. For than one activity can trigger the
/// event. By default all activities trigger the workflow. Use the Activities property to filter them.
/// </summary>
public record PullRequestReview : Webhook
{
    public enum Activity
    {
        Submitted,
        Edited,
        Dismissed,
    }

    /// <summary>
    /// List of activities that will be considered when the event is triggered.
    /// </summary>
    public List<Activity> Activities { get; } = new();
}

/// <summary>
/// Triggers the workflow for the 'pull_request_review_comment' event. For than one activity can trigger the
/// event. By default all activities trigger the workflow. Use the Activities property to filter them.
/// </summary>
public record PullRequestReviewComment : Webhook
{
    public enum Activity
    {
        Created,
        Edited,
        Deleted,
    }

    /// <summary>
    /// List of activities that will be considered when the event is triggered.
    /// </summary>
    public List<Activity> Activities { get; } = new();
}

/// <summary>
/// Triggers the workflow for the 'pull_request_target' event. For than one activity can trigger the
/// event. By default all activities trigger the workflow. Use the Activities property to filter them.
/// </summary>
public record PullRequestTarget : Webhook
{

    public enum Activity
    {
        Assigned,
        Unassigned,
        Labeled,
        Unlabeled,
        Opened,
        Edited,
        Closed,
        Reopened,
        Synchronize,
        ReadyForReview,
        Locked,
        Unlocked,
        ReviewRequested,
        ReviewRequestRemoved,
    }

    /// <summary>
    /// List of activities that will be considered when the event is triggered.
    /// </summary>
    public List<Activity> Activities { get; } = new();
}

/// <summary>
/// Triggers the workflow for the 'push' event.
/// </summary>
public record Push : Webhook;

/// <summary>
/// Triggers the workflow for the 'registry_package' event. For than one activity can trigger the
/// event. By default all activities trigger the workflow. Use the Activities property to filter them.
/// </summary>
public record RegistryPackage : Webhook
{

    public enum Activity
    {
        Published,
        Updated
    }

    /// <summary>
    /// List of activities that will be considered when the event is triggered.
    /// </summary>
    public List<Activity> Activities { get; } = new();
}

/// <summary>
/// Triggers the workflow for the 'release' event. For than one activity can trigger the
/// event. By default all activities trigger the workflow. Use the Activities property to filter them.
/// </summary>
public record Release : Webhook
{
    public enum Activity
    {
        Published,
        Unpublished,
        Created,
        Edited,
        Deleted,
        Prereleased,
        Released,
    }

    /// <summary>
    /// List of activities that will be considered when the event is triggered.
    /// </summary>
    public List<Activity> Activities { get; } = new();
}
public record Status : Webhook { }

/// <summary>
/// Triggers the workflow for the 'watch' event. For than one activity can trigger the
/// event. By default all activities trigger the workflow. Use the Activities property to filter them.
/// </summary>
public record Watch : Webhook
{
    public enum Activity
    {
        Started
    }

    /// <summary>
    /// List of activities that will be considered when the event is triggered.
    /// </summary>
    public List<Activity> Activities { get; } = new();
}

/// <summary>
/// Triggers the workflow for the 'workflow_run' event. For than one activity can trigger the
/// event. By default all activities trigger the workflow. Use the Activities property to filter them.
/// </summary>
public record WorkflowRun : Webhook
{
    public enum Activity
    {
        Completed,
        Requested,
    }

    /// <summary>
    /// List with the workflows that will trigger the event.
    /// </summary>
    [Required]
    public List<string> Workflows { get; } = new();

    /// <summary>
    /// List with the branches that will trigger the event.
    /// </summary>
    [Required]
    public List<string> Branches { get; } = new();

    /// <summary>
    /// List of activities that will be considered when the event is triggered.
    /// </summary>
    public List<Activity> Activities { get; } = new();
}
