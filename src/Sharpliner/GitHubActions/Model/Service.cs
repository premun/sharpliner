using System;

namespace Sharpliner.GitHubActions;

/// <summary>
/// Represents the service container that will be used for the job
/// </summary>
public record Service
{
    /// <summary>
    /// Get the Id of the service.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Get/Set the container of the service.
    /// </summary>
    public Container? Container { get; set; }

    /// <summary>
    /// The service with the given Id and container will be used for the job.
    /// </summary>
    /// <param name="id">Id of the service.</param>
    /// <param name="container">Container of the service.</param>
    public Service(string id, Container? container = null)
    {
        if (string.IsNullOrEmpty(id))
        {
            throw new ArgumentNullException(nameof(id));
        }

        Id = id;
        Container = container;
    }
}
