using System.Collections.Generic;

namespace Sharpliner.GitHubActions;

public record StrategyConfiguration
{
    public Dictionary<string, object>? Configuration { get; set; }
    public Dictionary<string, string>? Variables { get; set; }
}

/// <summary>
/// Creates a matrix of the jobs. A matrix allows to create different jobs via
/// variable substitution.
/// </summary>
public record Strategy
{

    /// <summary>
    /// Fail the job if any of the configurations from the matrix fails.
    /// </summary>
    public bool FailFast { get; set; } = true; // the default value in github actions is true

    /// <summary>
    /// The maximum number of jobs that can run simultaneously. Byt default github trying to maximize the value.
    /// </summary>
    public int MaxParallel { get; set; } = int.MaxValue;

    /// <summary>
    /// Defines the default configurations to be included in the matrix.
    /// </summary>
    public Dictionary<string, List<object>> Configuration { get; set; } = new();

    /// <summary>
    /// List of extra configurations that will be included.
    /// </summary>
    public List<StrategyConfiguration> Include { get; set; } = new();

    /// <summary>
    /// List of configurations to be excluded.
    /// </summary>
    public List<StrategyConfiguration> Exclude { get; set; } = new();

    /// <summary>
    /// Returns if the max number of jobs is used.
    /// </summary>
    internal bool UseMaxJobs => MaxParallel == int.MaxValue;
}
