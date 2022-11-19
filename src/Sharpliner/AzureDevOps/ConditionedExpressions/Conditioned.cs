using System;
using System.Collections.Generic;
using System.Linq;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace Sharpliner.AzureDevOps.ConditionedExpressions;

public abstract record Conditioned : IYamlConvertible
{
    /// <summary>
    /// Evaluated textual representation of the condition, e.g. "ne('foo', 'bar')".
    /// </summary>
    internal IfCondition? Condition { get; set; }

    /// <summary>
    /// Pointer in case of nested conditional blocks.
    /// </summary>
    internal Conditioned? Parent { get; set; }

    /// <summary>
    /// In case we define multiple items inside one ${{ if }}, they are stored here.
    /// </summary>
    internal List<Conditioned> Definitions { get; } = new();

    /// <summary>
    /// When serializing, we need to distinguish whether serializing a list of items under a condition or just a value.
    /// </summary>
    internal bool IsList { get; set; } = false;

    protected Conditioned(IfCondition? condition)
    {
        Condition = condition;
    }

    public void Read(IParser parser, Type expectedType, ObjectDeserializer nestedObjectDeserializer)
        => throw new NotImplementedException();

    public abstract void Write(IEmitter emitter, ObjectSerializer nestedObjectSerializer);

    internal virtual void SetIsList(bool isList)
    {
        IsList = isList;

        foreach (var child in Definitions)
        {
            child.SetIsList(isList);
        }
    }

    /// <summary>
    /// This method is used for double-linking of the definition expression tree.
    /// </summary>
    /// <param name="condition">Parent condition</param>
    /// <param name="definition">Definition that was added below the condition</param>
    /// <returns>The conditioned definition coming out of the inputs</returns>
    internal static Conditioned<T> Link<T>(IfCondition condition, T definition)
    {
        var conditionedDefinition = new Conditioned<T>(definition, condition);
        condition.Parent?.Definitions.Add(conditionedDefinition);
        conditionedDefinition.Parent = condition.Parent;
        return conditionedDefinition;
    }

    /// <summary>
    /// This method is used for double-linking of the definition expression tree.
    /// </summary>
    /// <param name="condition">Parent condition</param>
    /// <param name="conditionedDefinition">Definition that was added below the condition</param>
    /// <returns>The conditioned definition coming out of the inputs</returns>
    internal static Conditioned<T> Link<T>(IfCondition condition, Conditioned<T> conditionedDefinition)
    {
        condition.Parent?.Definitions.Add(conditionedDefinition);
        conditionedDefinition.Parent = condition.Parent;
        return conditionedDefinition;
    }

    /// <summary>
    /// This method is used for double-linking of the definition expression tree.
    /// </summary>
    /// <param name="condition">Parent condition</param>
    /// <param name="items">Items to add to a condition</param>
    /// <returns>The conditioned definition coming out of the inputs</returns>
    internal static Conditioned<T> Link<T>(IfCondition condition, IEnumerable<Conditioned<T>> items)
    {
        var conditionedDefinition = new Conditioned<T>(default!, condition);
        conditionedDefinition.Definitions.AddRange(items);
        condition.Parent?.Definitions.Add(conditionedDefinition);
        conditionedDefinition.Parent = condition.Parent;
        return conditionedDefinition;
    }

    /// <summary>
    /// This method is used for double-linking of the definition expression tree.
    /// </summary>
    /// <param name="condition">Parent condition</param>
    /// <param name="template">Definition that was added below the condition</param>
    /// <returns>The conditioned definition coming out of the inputs</returns>
    internal static Conditioned<T> Link<T>(IfCondition condition, Template<T> template)
    {
        if (condition.Parent == null)
        {
            template.Condition = condition;
        }
        else
        {
            condition.Parent?.Definitions.Add(template);
            template.Parent = condition.Parent;
        }

        return template;
    }
}

/// <summary>
/// Represents an item that might or might have a condition.
/// Example of regular definition:
///     - task: publish
/// Example of conditioned definition:
///     - ${{ if eq(variables._RunAsInternal, True) }}:
///       name: NetCoreInternal-Pool
/// </summary>
public record Conditioned<T> : Conditioned
{
    // Make sure we can for example assign a string into ConditionedDefinition<string>
    public static implicit operator Conditioned<T>(T value) => new(definition: value);

    /// <summary>
    /// The actual definition (value).
    /// </summary>
    internal T? Definition { get; }

    internal Conditioned(T definition, IfCondition condition) : base(condition)
    {
        Definition = definition;
    }

    public Conditioned(T definition) : this()
    {
        Definition = definition;
    }

    protected Conditioned(IfCondition? condition) : base(condition)
    {
    }

    protected Conditioned() : base((IfCondition?)null)
    {
    }

    public IfConditionBuilder<T> If => new(this);

    public Conditioned<T> EndIf
    {
        get
        {
            // If we're top-level, we create a fake new top with empty definition to collect all the definitions
            if (Parent == null)
            {
                Parent = new Conditioned<T>();
                Parent.Definitions.Add(this);
            }

            return Parent as Conditioned<T>
                ?? throw new InvalidOperationException("You have called EndIf on a top-level statement, EndIf can only be used to return from a nested definition");
        }
    }

    public IfCondition<T> Else
    {
        get
        {
            // If we're top-level, we create a fake new top with empty definition to collect all the definitions
            if (Parent == null)
            {
                Parent = new Conditioned<T>();
                Parent.Definitions.Add(this);
            }

            if (Condition == null)
            {
                throw new InvalidOperationException("No condition to match Else against");
            }

            return SharplinerConfiguration.Current.Serialization.UseElseExpression
                ? new ElseCondition<T>()
                {
                    Parent = Parent
                }
                : new IfNotCondition<T>(Condition)
                {
                    Parent = Parent
                };
        }
    }

    public override void Write(IEmitter emitter, ObjectSerializer nestedObjectSerializer)
    {
        if (IsList)
        {
            WriteList(emitter, nestedObjectSerializer);
        }
        else
        {
            WriteValue(emitter, nestedObjectSerializer);
        }
    }

    internal override void SetIsList(bool isList)
    {
        base.SetIsList(isList);

        if (Definition is Conditioned conditioned)
        {
            conditioned.SetIsList(isList);
        }
    }

    /// <summary>
    /// This method's responsibility is to serialize a single item which might or might not have conditions underneath.
    /// Example #1 (no condition)
    ///   name: value1
    ///
    /// Example #2 (conditions)
    ///   name:
    ///     ${{ if eq(...) }}
    ///       name2: value1
    ///     ${{ if ne(...) }}
    ///       name2: value2
    /// </summary>
    private void WriteValue(IEmitter emitter, ObjectSerializer nestedObjectSerializer)
    {
        if (Condition != null)
        {
            emitter.Emit(new Scalar(Condition.TagStart + Condition.WithoutTags() + Condition.TagEnd));
        }
        else if (Definitions.Count > 0)
        {
            emitter.Emit(new MappingStart());
        }

        // We are first serializing us. We can be
        //   - a condition-less definition (top level or leaf) => serialize value inside Definition
        //   - a template => serialize the special shape of template + parameters
        SerializeSelf(emitter, nestedObjectSerializer);

        // Otherwise, we expect a list of Definitions
        foreach (var childDefinition in Definitions)
        {
            nestedObjectSerializer(childDefinition);
        }

        if (Condition == null && Definitions.Count > 0)
        {
            emitter.Emit(new MappingEnd());
        }
    }

    /// <summary>
    /// This method's responsibility is to serialize a list of items which can share a common condition.
    /// </summary>
    private void WriteList(IEmitter emitter, ObjectSerializer nestedObjectSerializer)
    {
        if (Condition != null)
        {
            emitter.Emit(new MappingStart());
            emitter.Emit(new Scalar(Condition.TagStart + IfCondition.WithoutTags(Condition) + Condition.TagEnd));
            emitter.Emit(new SequenceStart(AnchorName.Empty, TagName.Empty, true, SequenceStyle.Block));
        }

        // We are first serializing us. We can be
        //   - a condition-less definition (top level or leaf) => serialize value inside Definition
        //   - a template => serialize the special shape of template + parameters
        SerializeSelf(emitter, nestedObjectSerializer);

        // Otherwise, we expect a list of Definitions
        foreach (var childDefinition in Definitions)
        {
            nestedObjectSerializer(childDefinition);
        }

        if (Condition != null)
        {
            emitter.Emit(new SequenceEnd());
            emitter.Emit(new MappingEnd());
        }
    }

    protected virtual void SerializeSelf(IEmitter emitter, ObjectSerializer nestedObjectSerializer)
    {
        if (Definition != null)
        {
            nestedObjectSerializer(Definition);
        }
    }

    internal IReadOnlyCollection<T> FlattenDefinitions()
    {
        var definitions = new List<T>();

        if (Definition is not null)
        {
            definitions.Add(Definition);
        }

        definitions.AddRange(
            Definitions
                .SelectMany(s => (s as Conditioned<T>)?.FlattenDefinitions()!)
                .Where(s => s is not null));

        return definitions;
    }

    public override string ToString() =>
        $"{(Condition == null ? "" : Condition + ": ")}{typeof(T).Name} " +
        $"with {(Definition == null ? Definitions.Count : Definitions.Count + 1)} items";
}
