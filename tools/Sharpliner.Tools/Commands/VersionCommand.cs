using System;
using Mono.Options;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace Sharpliner.Tools.Commands;

internal class VersionCommand : Command
{
    public VersionCommand() : base("version") { }

    public override int Invoke(IEnumerable<string> arguments)
    {
        var version = GetAssemblyVersion();
        Console.WriteLine(version.ProductVersion);
        return 0;
    }

    public static FileVersionInfo GetAssemblyVersion() => FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
}
