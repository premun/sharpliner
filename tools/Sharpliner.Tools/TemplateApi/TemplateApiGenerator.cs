using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Sharpliner.Tools.TemplateApi;

public class TemplateApiGenerator
{
    private static string GetNewContent(string namespaceName, string className) =>
        $$"""
        using Sharpliner.AzureDevOps;

        namespace {{namespaceName}};

        public static class {{className}}
        {
        }
        """;

    private static readonly IDeserializer s_deserializer = new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .Build();

    public string[] AddOrUpdateApi(string namespaceName, string className, string? content, string templatePath)
    {
        TemplateDefinition template;
        using (var reader = File.OpenText(templatePath))
        {
            template = s_deserializer.Deserialize<TemplateDefinition>(reader);
        }

        content ??= GetNewContent(namespaceName, className);

        var newLine = content.Contains("\r\n") ? "\r\n" : Environment.NewLine;
        var lines = content.Split(newLine).ToList();

        return lines.ToArray();
    }

    class TemplateDefinition
    {
        public Dictionary<string, object>? Parameters { get; set; }
        public List<Dictionary<object, object>>? Stages { get; set; }
        public List<Dictionary<object, object>>? Jobs { get; set; }
        public List<Dictionary<object, object>>? Steps { get; set; }
        public List<Dictionary<object, object>>? Variables { get; set; }
    }

    private class TemplateParameter
    {
        public string? Name { get; set; }
        public string? Type { get; set; }
        public object? Default { get; set; }
    }
}
