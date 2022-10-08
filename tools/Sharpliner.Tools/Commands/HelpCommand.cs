using System;
using System.Collections.Generic;
using System.Linq;

namespace Sharpliner.Tools.Commands;

internal class HelpCommand : Mono.Options.HelpCommand
{
    public override int Invoke(IEnumerable<string> arguments)
    {
        string[] args = arguments.ToArray();

        if (args.Length == 0)
        {
            base.Invoke(arguments);
            return (int)ExitCode.HELP_SHOWN;
        }

        PrintCommandHelp(args[0].ToLowerInvariant());
        return (int)ExitCode.HELP_SHOWN;
    }

    private static void PrintCommandHelp(string name)
    {
        var commandSet = Program.GetCommandSet();
        var command = commandSet.Where(c => c.Name == name).FirstOrDefault();
        if (command != null)
        {
            command.Invoke(new string[] { "--help" });
            return;
        }

        Console.WriteLine($"Unknown command '{name}'.{Environment.NewLine}");
        commandSet.Run(new string[] { "help" });
    }
}
