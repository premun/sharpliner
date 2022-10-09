using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sharpliner.Tools.CommandArguments;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Sharpliner.Tools.Commands;

internal class TemplateApiCommand : BaseCommand<TemplateApiCommandArguments>
{
    private const string CommandHelp = "Parses an existing YAML template and creates C# API for referencing it";

    private static string GetNewContent(string namespaceName, string className) =>
        $$"""
        using Sharpliner.AzureDevOps;

        namespace {{namespaceName}};

        public static class {{className}}
        {
        }
        """;

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
            ProcessTemplate(logger, path);
            logger.LogInformation("API for template {path} created", path);
        }

        return Task.FromResult(ExitCode.SUCCESS);
    }

    private void ProcessTemplate(ILogger logger, string path)
    {
        var parentFolder = Path.GetDirectoryName(Arguments.OutputPath);
        Directory.CreateDirectory(parentFolder!);

        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        var template = deserializer.Deserialize<TemplateDefinition>(File.OpenText(path));

        var content = File.Exists(Arguments.OutputPath)
            ? File.ReadAllText(Arguments.OutputPath)
            : GetNewContent(Arguments.Namespace, Path.GetFileNameWithoutExtension(path));

        var newLine = content.Contains("\r\n") ? "\r\n" : Environment.NewLine;

        var lines = content.Split(newLine);

        File.WriteAllLines(path, lines);
    }

    private class TemplateDefinition
    {
        public List<TemplateParameter>? Parameters { get; set; }
        public Dictionary<object, object>? Stages { get; set; }
        public Dictionary<object, object>? Jobs { get; set; }
        public Dictionary<object, object>? Steps { get; set; }
        public Dictionary<object, object>? Variables { get; set; }
    }

    private class TemplateParameter
    {
        public string? Name { get; set; }
        public string? Type { get; set; }
        public object? Default { get; set; }
    }
}
