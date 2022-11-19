using System;
using System.Linq;
using Sharpliner.AzureDevOps.ConditionedExpressions;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace Sharpliner.AzureDevOps;

/// <summary>
/// Represents a template refernce
/// </summary>
/// <typeparam name="T">Part of the pipeline this template substitutes (allowed are stage, job, step, variable)</typeparam>
public record Template<T> : Conditioned<T>
{
    private readonly string _path;
    public TemplateParameters Parameters { get; init; } = new();

    /// <summary>
    /// We shouldn't allow any type of template as Azure DevOps doesn't accept it.
    /// Use methods such as StageTemplate, JobTemplate, StepTemplate or VariableTemplate.
    /// </summary>
    /// <param name="path">Path to the yaml file</param>
    /// <param name="parameters">Parameters to be passed to the template</param>
    internal Template(string path, TemplateParameters? parameters = null) : this(null, path, parameters)
    {
    }

    internal Template(IfCondition? condition, string path, TemplateParameters? parameters = null) : base(condition)
    {
        if (string.IsNullOrEmpty(path))
        {
            throw new ArgumentException($"'{nameof(path)}' cannot be null or empty.", nameof(path));
        }

        _path = path;

        if (parameters != null)
        {
            Parameters = parameters;
        }
    }

    protected override void SerializeSelf(IEmitter emitter, ObjectSerializer nestedObjectSerializer)
    {
        emitter.Emit(new MappingStart());
        emitter.Emit(new Scalar("template"));
        emitter.Emit(new Scalar(_path));

        if (Parameters != null && Parameters.Any())
        {
            emitter.Emit(new Scalar("parameters"));
            nestedObjectSerializer(Parameters);
        }

        emitter.Emit(new MappingEnd());
    }
}
