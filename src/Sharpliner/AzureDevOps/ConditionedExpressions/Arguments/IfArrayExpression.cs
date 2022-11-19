using OneOf;

namespace Sharpliner.AzureDevOps.ConditionedExpressions.Arguments;

[GenerateOneOf]
public partial class IfArrayExpression : OneOfBase<string[], object[], ParameterReference[], VariableReference[]>
{
}
