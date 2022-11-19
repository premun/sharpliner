using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Sharpliner.AzureDevOps.ConditionedExpressions;

/// <summary>
/// This class is here only to override the Add() which is used for definition.
/// </summary>
public class ConditionedList<T> : List<Conditioned<T>>
{
    public ConditionedList()
    {
    }

    protected ConditionedList(IEnumerable<T> values)
        : base(values.Select(v => new Conditioned<T>(v)))
    {
    }

    public new void Add(Conditioned<T> item)
    {
        // When we define a tree of conditional definitions, the expression returns
        // the leaf definition so we have to move up to the top-level definition
        while (item.Parent is Conditioned<T> parent)
        {
            item = parent;
        }

        item.SetIsList(true);
        base.Add(item);
    }

    // Make sure we can for example assign common collection types into ConditionedList
    public static implicit operator ConditionedList<T>(List<T> list) => new(list);
    public static implicit operator ConditionedList<T>(T[] values) => new(values);
    public static implicit operator ConditionedList<T>(ReadOnlyCollection<T> values) => new(values);
}
