using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Sharpliner.Tools.Arguments;

namespace Sharpliner.Tools.CommandArguments;

public interface ICommandArguments
{
    VerbosityArgument Verbosity { get; set; }
    HelpArgument ShowHelp { get; }
    IEnumerable<Argument> GetCommandArguments();
    void Validate();
}

public abstract class CommandArguments : ICommandArguments
{
    public VerbosityArgument Verbosity { get; set; } = new(LogLevel.Information);
    public HelpArgument ShowHelp { get; } = new();

    public IEnumerable<Argument> GetCommandArguments() => GetArguments().Concat(new Argument[]
    {
        Verbosity,
        ShowHelp,
    });

    public virtual void Validate()
    {
        foreach (var arg in GetCommandArguments())
        {
            arg.Validate();
        }
    }

    /// <summary>
    /// Returns additional option for your specific command.
    /// </summary>
    protected abstract IEnumerable<Argument> GetArguments();
}
