using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sharpliner.Tools.CommandArguments;

namespace Sharpliner.Tools.Commands;

internal class TemplateApiCommand : BaseCommand<TemplateApiCommandArguments>
{
    private const string CommandHelp = "Parses an existing YAML template and creates C# API for referencing it";

    protected override TemplateApiCommandArguments Arguments { get; } = new();
    protected override string CommandUsage { get; } = "template-api [OPTIONS]";
    protected override string CommandDescription { get; } = CommandHelp;

    public TemplateApiCommand(IServiceCollection services) : base("template-api", false, services, CommandHelp)
    {
    }

    protected override Task<ExitCode> InvokeInternal(ILogger logger)
    {
        throw new NotImplementedException();
    }
}
