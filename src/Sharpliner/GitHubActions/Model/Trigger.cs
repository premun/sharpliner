using System.Collections.Generic;

namespace Sharpliner.GitHubActions;

/// <summary>
/// Representation of all the possible triggers that a GitHub workflow can react too.
/// </summary>
public class Trigger
{
    /// <summary>
    /// Collection of triggers that execute the workflow based on a schedule.
    /// </summary>
    public List<Schedule> Schedules { get; } = new();

    /// <summary>
    /// Triggers that required a manual intervention to execute the workflow. The intervention can be done by
    /// a human or an external service.
    /// </summary>
    public Manual Manuals { get; } = new();

    /// <summary>
    /// Collection of triggers that executed the workflow based on a webhook from the GitHup API.
    /// </summary>
    public List<Webhook> Webhooks { get; } = new();
}
