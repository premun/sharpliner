using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sharpliner.GitHubActions;

/// <summary>
/// Abstract class that represents all the triggers that are initiated manually by a user.
/// </summary>
public record Manual : Event
{
    /// <summary>
    /// Trigger that will execute the workflow via the GitHub web manually.
    /// </summary>
    public WorkflowDispatch? WorkflowDispatch { get; set; }

    /// <summary>
    /// Trigger that will execute the workflow via an external service.
    /// </summary>
    public RepositoryDispatch? RepositoryDispatch { get; set; }
}

/// <summary>
/// Trigger that allows to execute a workflow manually using custom inputs. These inputs can be optional or required
/// and can later be accessed within the workflow.
/// </summary>
public record WorkflowDispatch
{
    /// <summary>
    /// Represents a workflow input when it is manually triggered. It must have a nam
    /// </summary>
    public record Input
    {
        /// <summary>
        /// Create a new input to be used when the workflow is manually triggered.
        /// </summary>
        /// <param name="name">The name of the input field. This must be unique.</param>
        public Input(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Get the name of the input field.
        /// </summary>
        [Required]
        public string Name { get; }

        /// <summary>
        /// A human readable description of the use of the input.
        /// </summary>
        public string? Description { get; init; }

        /// <summary>
        /// The default value to be used.
        /// </summary>
        public string? Default { get; init; }

        /// <summary>
        /// Specifies if the input must contain a value or not.
        /// </summary>
        public bool IsRequired { get; init; }
    }

    /// <summary>
    /// List of inputs to be used with the workflow when it is manually triggered.
    /// </summary>
    public List<Input> Inputs { get; } = new();
}

/// <summary>
/// Triggers a workflow when the repository_dispatch has been triggered by an external service.
/// </summary>
public record RepositoryDispatch
{
    /// <summary>
    /// List of activities that will be considered when the event is triggered. This are provided by the
    /// external service and therefore cannot be validated.
    /// </summary>
    public List<string> Activities { get; } = new();
}
