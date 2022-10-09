using System.Collections.Generic;
using Sharpliner.Tools.Arguments;

namespace Sharpliner.Tools.CommandArguments;

internal class TemplateApiCommandArguments : CommandArguments
{
    public OutputPathArgument OutputPath { get; } = new();
    public NamespaceArgument Namespace { get; } = new();

    protected override IEnumerable<Argument> GetArguments() => new Argument[]
    {
        OutputPath,
        Namespace,
    };
}
