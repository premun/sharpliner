using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Mono.Options;
using Sharpliner.Tools.Commands;

namespace Sharpliner.Tools;

public static class Program
{
    /// <summary>
    /// The verbatim "--" argument used for pass-through args is removed by Mono.Options when parsing CommandSets,
    /// so in Program.cs, we temporarily replace it with this string and then recognize it back here.
    /// </summary>
    public const string VerbatimArgumentPlaceholder = "[[%verbatim_argument%]]";

    public static int Main(string[] args)
    {
        bool shouldOutput = !IsOutputSensitive(args);

        if (shouldOutput)
        {
            Console.WriteLine(
                $"[{VersionCommand.GetAssemblyVersion().ProductVersion}] " +
                "command issued: " + string.Join(' ', args));
        }

        if (args.Length > 0)
        {
            // Mono.Options wouldn't allow "--" so we will temporarily rename it and parse it ourselves later
            args = args.Select(a => a == "--" ? VerbatimArgumentPlaceholder : a).ToArray();
        }

        var commands = GetCommandSet();
        int result = commands.Run(args);

        string? exitCodeName = null;
        if (args.Length > 0 && result != 0 && Enum.IsDefined(typeof(ExitCode), result))
        {
            exitCodeName = $" ({(ExitCode)result})";
        }

        if (shouldOutput)
        {
            Console.WriteLine($"Exit code: {result}{exitCodeName}");
        }

        return result;
    }

    public static CommandSet GetCommandSet()
    {
        var commandSet = new CommandSet("sharpliner.tools")
        {
            new TemplateApiCommand(new ServiceCollection()),
            new Commands.HelpCommand(),
            new VersionCommand()
        };

        return commandSet;
    }

    /// <summary>
    /// Returns true when the command outputs data suitable for parsing and we should keep the output clean.
    /// </summary>
    private static bool IsOutputSensitive(string[] args)
    {
        if (args.Length > 0 && args[0] == "version")
        {
            return true;
        }

        return false;
    }
}
