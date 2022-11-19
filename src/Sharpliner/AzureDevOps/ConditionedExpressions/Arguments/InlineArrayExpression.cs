using OneOf;

namespace Sharpliner.AzureDevOps.ConditionedExpressions.Arguments;

[GenerateOneOf]
public partial class InlineArrayExpression : OneOfBase<string[], object[], ParameterReference[], VariableReference[]>
{
}
