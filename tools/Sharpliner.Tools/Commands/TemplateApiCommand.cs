using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sharpliner.Tools.CommandArguments;
using Sharpliner.Tools.TemplateApi;

namespace Sharpliner.Tools.Commands;

internal class TemplateApiCommand : BaseCommand<TemplateApiCommandArguments>
{
    private const string CommandHelp = "Parses an existing YAML template and creates C# API for referencing it";

    private readonly TemplateApiGenerator _templateGenerator = new();

    protected override TemplateApiCommandArguments Arguments { get; } = new();
    protected override string CommandUsage { get; } = "template-api [OPTIONS] [TEMPLATE1.yml] [TEMPLATE2.yml] ...";
    protected override string CommandDescription { get; } = CommandHelp;

    public TemplateApiCommand(IServiceCollection services) : base("template-api", true, services, CommandHelp)
    {
    }

    protected override Task<ExitCode> InvokeInternal(ILogger logger)
    {
        if (!ExtraArguments.Any())
        {
            throw new ArgumentException("Missing positional arguments - template files to process");
        }

        foreach (var path in ExtraArguments)
        {
            if (!File.Exists(path))
            {
                throw new ArgumentException($"Template path `{path}` does not exist");
            }

            logger.LogInformation("Processing {path}..", path);
            ProcessTemplate(path);
            logger.LogInformation("API for template {path} created in {destination}", path, Arguments.OutputPath);
        }

        return Task.FromResult(ExitCode.SUCCESS);
    }

    private void ProcessTemplate(string path)
    {
        string? content = File.Exists(Arguments.OutputPath) ? File.ReadAllText(Arguments.OutputPath) : null;
        string className = Path.GetFileNameWithoutExtension(Arguments.OutputPath);

        var lines = _templateGenerator.AddOrUpdateApi(Arguments.Namespace, className, content, path);

        var parentFolder = Path.GetDirectoryName(Arguments.OutputPath);
        Directory.CreateDirectory(parentFolder!);
        File.WriteAllLines(Arguments.OutputPath, lines);
    }
}
