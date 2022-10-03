using System;
using System.Collections.Generic;
using Sharpliner.Common;

namespace Sharpliner.GitHubActions;

/// <summary>
/// Inherit from this class to define a GitHub workflow.
/// </summary>
public abstract class WorkflowDefinition : ISharplinerDefinition
{
    /// <summary>
    /// Path to the YAML file where this pipeline will be exported to.
    /// When you build the project, the pipeline will be saved into a file on this path.
    /// Example: "pipelines/ci.yaml"
    /// </summary>
    public abstract string TargetFile { get; }

    public virtual TargetPathType TargetPathType => TargetPathType.RelativeToCurrentDir;

    /// <summary>
    /// Header that will be shown at the top of the generated YAML file
    ///
    /// Leave empty array to omit file header.
    /// </summary>
    public virtual string[]? Header => SharplinerPublisher.GetDefaultHeader(GetType());

    /// <summary>
    /// Define the pipeline by implementing this field.
    /// </summary>
    public abstract Workflow Workflow { get; }

    // TODO: Add validations for workflows
    /// <summary>
    /// Returns the list of validations that should be run on the definition (e.g. wrong dependsOn, artifact name typos..).
    /// </summary>
    public IReadOnlyCollection<IDefinitionValidation> Validations => Array.Empty<IDefinitionValidation>();

    /// <summary>
    /// Serializes this pipeline into YAML.
    /// </summary>
    public string Serialize() => SharplinerSerializer.Serialize(Workflow);
}
