using OneOf;

namespace Sharpliner.AzureDevOps.ConditionedExpressions.Arguments;

[GenerateOneOf]
public partial class InlineExpression : OneOfBase<string, ParameterReference, VariableReference>
{
}
