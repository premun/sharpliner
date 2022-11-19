using Sharpliner.AzureDevOps.ConditionedExpressions;

namespace Sharpliner.AzureDevOps;

public abstract class InlineCondition : Condition
{
}

public abstract class InlineCondition<T> : InlineCondition
{
    protected InlineCondition(Conditioned<T>? parent = null)
    {
        Parent = parent;
    }
}
