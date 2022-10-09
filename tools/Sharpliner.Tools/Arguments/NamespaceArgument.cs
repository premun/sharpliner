namespace Sharpliner.Tools.Arguments;

public class NamespaceArgument : RequiredStringArgument
{
    public NamespaceArgument()
        : base("namespace=", "Name of the namespace used in the generated code. Defaults to 'Pipelines'", "Pipelines")
    {
    }
}

