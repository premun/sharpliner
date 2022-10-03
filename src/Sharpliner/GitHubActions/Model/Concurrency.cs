using System;

namespace Sharpliner.GitHubActions;

public record Concurrency
{
    /// <summary>
    /// Represents a concurrency group that will be used to coordinate the execution of the workflow. This will
    /// ensure that only one workflow is executed at a specific time.
    /// </summary>
    /// <param name="name">Name or expression of the concurrency context.</param>
    /// <param name="cancelInProgress">True if already in process workflows should be canceled.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public Concurrency(string name, bool cancelInProgress = false)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        CancelInProgress = cancelInProgress;
    }

    /// <summary>
    /// Get the name of the concurrency group.
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    /// Get/Set if other running workflows should be canceled.
    /// </summary>
    public bool CancelInProgress { get; set; } = false;
}
