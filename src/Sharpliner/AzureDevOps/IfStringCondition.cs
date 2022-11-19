using Sharpliner.AzureDevOps.ConditionedExpressions;
using Sharpliner.AzureDevOps.ConditionedExpressions.Arguments;

namespace Sharpliner.AzureDevOps;

public abstract class IfStringCondition : IfCondition
{
    private readonly string _keyword;
    private readonly string _one;
    private readonly string _two;

    protected IfStringCondition(string keyword, IfArrayExpression one, IfExpression two)
        : this(keyword, Serialize(one), two)
    {
    }

    protected IfStringCondition(string keyword, IfExpression one, IfArrayExpression two)
        : this(keyword, one, Serialize(two))
    {
    }

    protected IfStringCondition(string keyword, IfExpression one, IfExpression two)
    {
        _keyword = keyword;
        _one = Serialize(one);
        _two = Serialize(two);
    }

    protected static string Serialize(IfArrayExpression arrayValue)
    {
        return IfStringConditionHelper.Serialize(arrayValue);
    }

    protected static string Serialize(IfExpression stringOrVariableOrParameter)
    {
        return IfStringConditionHelper.Serialize(stringOrVariableOrParameter);
    }

    internal override string Serialize() => WrapTag($"{_keyword}({WrapQuotes(_one)}, {WrapQuotes(_two)})");
}

public abstract class IfStringCondition<T> : IfCondition<T>
{
    private readonly string _keyword;
    private readonly string _one;
    private readonly string _two;

    protected IfStringCondition(string keyword, IfArrayExpression one, IfExpression two, Conditioned<T>? parent = null)
        : this(keyword, Serialize(one), two, parent)
    {
    }

    protected IfStringCondition(string keyword, IfExpression one, IfArrayExpression two, Conditioned<T>? parent = null)
        : this(keyword, one, Serialize(two), parent)
    {
    }

    protected IfStringCondition(string keyword, IfExpression one, IfExpression two, Conditioned<T>? parent = null)
    {
        _keyword = keyword;
        _one = Serialize(one);
        _two = Serialize(two);
        Parent = parent;
    }

    protected static string Serialize(IfArrayExpression arrayValue)
    {
        return IfStringConditionHelper.Serialize(arrayValue);
    }

    protected static string Serialize(IfExpression stringOrVariableOrParameter)
    {
        return IfStringConditionHelper.Serialize(stringOrVariableOrParameter);
    }

    internal override string Serialize() => WrapTag($"{_keyword}({WrapQuotes(_one)}, {WrapQuotes(_two)})");
}
