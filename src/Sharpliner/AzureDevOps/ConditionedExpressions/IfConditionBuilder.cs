using Sharpliner.AzureDevOps.ConditionedExpressions.Arguments;

namespace Sharpliner.AzureDevOps.ConditionedExpressions;

/// <summary>
/// The builder is what let's us start the definition with the "If."
/// and then forces us to add a condition. The condition then forces us to add
/// an actual definition.
/// </summary>
public class IfConditionBuilder
{
    internal Conditioned? Parent { get; }

    internal IfConditionBuilder(Conditioned? parent = null)
    {
        Parent = parent;
    }

    /// <summary>
    /// Specify any condition
    /// </summary>
    public IfCondition Condition(string condition)
        => Link(new IfCustomCondition(condition));

    public IfCondition Equal(IfCondition condition)
        => Link(condition);

    public IfCondition NotEqual(IfCondition condition)
        => Link(condition);

    public IfCondition Equal(IfExpression expression1, IfExpression expression2)
        => Link(new IfEqualityCondition(true, expression1, expression2));

    public IfCondition NotEqual(IfExpression expression1, IfExpression expression2)
        => Link(new IfEqualityCondition(false, expression1, expression2));

    public IfCondition And(params IfCondition[] expressions)
        => Link(new IfAndCondition(expressions));

    public IfCondition Or(params IfCondition[] expressions)
        => Link(new IfOrCondition(expressions));

    public IfCondition And(params string[] expressions)
        => Link(new IfAndCondition(expressions));

    public IfCondition Or(params string[] expressions)
        => Link(new IfOrCondition(expressions));

    public IfCondition Xor(IfCondition expression1, IfCondition expression2)
        => Link(new IfXorCondition(expression1, expression2));

    public IfCondition Xor(string expression1, string expression2)
        => Link(new IfXorCondition(expression1, expression2));

    public IfCondition StartsWith(IfExpression needle, IfExpression haystack)
        => new IfStartsWithCondition(needle, haystack);

    public IfCondition EndsWith(IfExpression needle, IfExpression haystack)
        => new IfEndsWithCondition(needle, haystack);

    public IfCondition Contains(IfExpression needle, IfExpression haystack)
        => new IfContainsCondition(needle, haystack);

    public IfCondition ContainsValue(IfExpression needle, params IfExpression[] haystack)
        => new IfContainsValueCondition(needle, haystack);

    public IfCondition In(IfExpression needle, params IfExpression[] haystack)
        => new IfInCondition(needle, haystack);

    public IfCondition NotIn(IfExpression needle, params IfExpression[] haystack)
        => new IfNotInCondition(needle, haystack);

    public IfCondition Greater(IfExpression first, IfExpression second)
        => Link(new IfGreaterCondition(first, second));

    public IfCondition Less(IfExpression first, IfExpression second)
        => Link(new IfLessCondition(first, second));

    public IfCondition IsBranch(IfExpression branchName)
        => Link(new IfBranchCondition(branchName, true));

    public IfCondition IsNotBranch(IfExpression branchName)
        => Link(new IfBranchCondition(branchName, false));

    public IfCondition IsPullRequest
        => Link(new IfBuildReasonCondition(new string("PullRequest"), true));

    public IfCondition IsNotPullRequest
        => Link(new IfBuildReasonCondition(new string("PullRequest"), false));

    private IfCondition Link(IfCondition condition)
    {
        condition.Parent = Parent;
        return condition;
    }
}

public class IfConditionBuilder<T>
{
    internal Conditioned<T>? Parent { get; }

    internal IfConditionBuilder(Conditioned<T>? parent = null)
    {
        Parent = parent;
    }

    public IfCondition<T> Equal(string condition)
        => Link(new IfCustomCondition<T>(condition));

    public IfCondition<T> Equal(IfExpression expression1, IfExpression expression2)
        => Link(new IfEqualityCondition<T>(true, expression1, expression2));

    public IfCondition<T> NotEqual(IfExpression expression1, IfExpression expression2)
    {
        return Link(new IfEqualityCondition<T>(false, expression1, expression2));
    }

    public IfCondition<T> And(params IfCondition[] expressions)
        => Link(new IfAndCondition<T>(expressions));

    public IfCondition<T> Or(params IfCondition[] expressions)
        => Link(new IfOrCondition<T>(expressions));

    public IfCondition<T> Xor(IfCondition expression1, IfCondition expression2)
        => Link(new IfXorCondition<T>(expression1, expression2));

    public IfCondition<T> Xor(string expression1, string expression2)
        => Link(new IfXorCondition<T>(expression1, expression2));

    public IfCondition<T> And(params string[] expressions)
        => Link(new IfAndCondition<T>(expressions));

    public IfCondition<T> Or(params string[] expressions)
        => Link(new IfOrCondition<T>(expressions));

    public IfCondition<T> StartsWith(IfExpression needle, IfExpression haystack)
        => new IfStartsWithCondition<T>(needle, haystack);

    public IfCondition<T> EndsWith(IfExpression needle, IfExpression haystack)
        => new IfEndsWithCondition<T>(needle, haystack);

    public IfCondition<T> Contains(IfExpression needle, IfExpression haystack)
        => new IfContainsCondition<T>(needle, haystack);

    public IfCondition<T> In(IfExpression needle, IfExpression haystack)
        => new IfInCondition<T>(needle, haystack);

    public IfCondition<T> NotIn(IfExpression needle, params IfExpression[] haystack)
        => new IfNotInCondition<T>(needle, haystack);

    public IfCondition<T> ContainsValue(IfExpression needle, params IfExpression[] haystack)
        => new IfContainsValueCondition<T>(needle, haystack);

    public IfCondition<T> IsBranch(IfExpression branchName)
        => Link(new IfBranchCondition<T>(branchName, true));

    public IfCondition<T> IsNotBranch(IfExpression branchName)
        => Link(new IfBranchCondition<T>(branchName, false));

    public IfCondition<T> Greater(IfExpression first, IfExpression second)
        => Link(new IfGreaterCondition<T>(first, second));

    public IfCondition<T> Less(IfExpression first, IfExpression second)
        => Link(new IfLessCondition<T>(first, second));

    public IfCondition<T> IsPullRequest
        => Link(new IfBuildReasonCondition<T>("PullRequest", true));

    public IfCondition<T> IsNotPullRequest
        => Link(new IfBuildReasonCondition<T>("PullRequest", false));

    private IfCondition<T> Link(IfCondition<T> condition)
    {
        condition.Parent = Parent;
        return condition;
    }
}
