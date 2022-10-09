namespace Sharpliner.Tools.Arguments;

public class OutputPathArgument : RequiredStringArgument
{
    public OutputPathArgument()
        : base("o|output=", "Path to the output file where the generated code will be written. Defaults to 'Templates.cs'", "Templates.cs")
    {
    }
}

