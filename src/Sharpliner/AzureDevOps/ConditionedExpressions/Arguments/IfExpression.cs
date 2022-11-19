using OneOf;

namespace Sharpliner.AzureDevOps.ConditionedExpressions.Arguments;

[GenerateOneOf]
public partial class IfExpression : OneOfBase<string, ParameterReference, VariableReference>
{
}
