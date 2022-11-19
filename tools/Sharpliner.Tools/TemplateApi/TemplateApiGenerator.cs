using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Sharpliner.Tools.TemplateApi;

public partial class TemplateApiGenerator
{
    private string _newLine = Environment.NewLine;

    private static readonly IDeserializer s_deserializer = new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .Build();

    public List<string> AddOrUpdateApi(string namespaceName, string className, string? content, string templatePath)
    {
        ParsedTemplate template;
        using (var reader = File.OpenText(templatePath))
        {
            template = s_deserializer.Deserialize<ParsedTemplate>(reader);
        }

        content ??= GetNewContent(namespaceName, className);

        _newLine = content.Contains("\r\n") ? "\r\n" : Environment.NewLine;
        var lines = content.Split(_newLine).ToList();

        var start = lines.FindIndex(l => l.Contains("public static class " + className)) + 2;
        var bodyLength = lines.Count - start - 1;

        var body = lines.Skip(start).Take(bodyLength).ToList();
        var newBody = AddOrUpdateMethod(templatePath, template, body);

        var newContent = lines
            .Take(start)
            .Concat(newBody)
            .Concat(lines.Skip(start + bodyLength))
            .ToArray();

        var result = new List<string>();
        var previous = "_";
        foreach (var line in newContent)
        {
            if (string.IsNullOrEmpty(previous) && string.IsNullOrEmpty(line))
            {
                continue;
            }

            if (string.IsNullOrEmpty(previous) && line == "}")
            {
                result.RemoveAt(result.Count - 1);
            }

            previous = line;
            result.Add(line);
        }

        return result;
    }

    private List<string> AddOrUpdateMethod(string templatePath, ParsedTemplate template, List<string> body)
    {
        var methodName = CreateMethodName(templatePath);
        var arguments = GetArguments(template);
        var relativePath = GetRelativePath(templatePath);
        var methodBody = CreateMethod(relativePath, methodName, InferType(template), arguments);

        var methodTag = $"// {relativePath}";
        var startIndex = body.FindIndex(s => s.Contains(methodTag));
        var endIndex = body.FindIndex(startIndex + 1, s => s.Length == 0 || s.Contains(");") || s == "}");

        if (startIndex != -1)
        {
            body.RemoveRange(startIndex, endIndex - startIndex + 1);
        }

        return body
            .Take(startIndex)
            .Append($"    {methodTag}")
            .Concat(methodBody)
            .Concat(body.Skip(startIndex))
            .ToList();
    }

    private List<string> CreateMethod(string relativePath, string methodName, string type, TemplateReferenceArgument[] arguments)
    {
        var args = arguments.Select(arg => $"{arg.Type} {arg.Name}{(arg.Default is not null ? $" = {arg.Default}" : string.Empty)}").ToList();

        // Break and indent args?
        var methodArgs = args.Count > 3
            ? $"{_newLine}        " + string.Join($",{_newLine}        ", args)
            : string.Join(", ", args);

        var signature = $"    public static Template<{type}> {methodName}({methodArgs}) => new(\"{relativePath}\"";
        
        if (!arguments.Any())
        {
            signature += ");";
        }
        else
        {
            signature += ", new()";
        }

        var body = new List<string>
        {
            signature,
        };

        if (arguments.Any())
        {
            body.Add("    {");
            body.AddRange(arguments.Select(a => $"        {{ \"{ a.Name }\", {a.Name} }},"));
            body.Add("    });");
        }

        body.Add(string.Empty);

        return body;
    }

    private static TemplateReferenceArgument[] GetArguments(ParsedTemplate parsedTemplate)
    {
        if (parsedTemplate.Parameters == null)
        {
            return Array.Empty<TemplateReferenceArgument>();
        }

        return parsedTemplate.Parameters
            .Select(pair => GetArgument(pair.Key, pair.Value))
            .Order(TemplateReferenceArgumentComparer.Instance)
            .ToArray();
    }

    private static TemplateReferenceArgument GetArgument(string name, object value)
    {
        var argument = new TemplateReferenceArgument()
        {
            Name = name
        };

        (argument.Type, argument.Default) = value switch
        {
            int i => ("int", i),
            double d => ("double", d),
            float f => ("float", f),
            "true" or "false" => ("bool", ((string)value).ToLowerInvariant()),
            string s => ("string", s is not null ? $"\"{s}\"" : null),
            bool b => ("bool", b),
            _ => ("MISSING_TYPE", (object?)null)
        };

        return argument;
    }

    private static string GetRelativePath(string templatePath)
    {
        var gitRoot = new DirectoryInfo(Path.GetDirectoryName(templatePath)!);
        while (!Directory.Exists(Path.Combine(gitRoot.FullName, ".git")))
        {
            gitRoot = gitRoot.Parent;

            if (gitRoot == null)
            {
                throw new Exception($"Failed to find git repository in {Directory.GetParent(Assembly.GetExecutingAssembly().Location)?.FullName}");
            }
        }

        return '/' + templatePath
            .Substring(gitRoot.FullName.Length)
            .TrimStart(Path.DirectorySeparatorChar)
            .Replace('\\', '/');
    }

    private static string InferType(ParsedTemplate template)
    {
        if (template.Stages != null)
        {
            return "Stage";
        }

        if (template.Jobs != null)
        {
            return "JobBase";
        }

        if (template.Steps != null)
        {
            return "Step";
        }

        if (template.Variables != null)
        {
            return "VariableBase";
        }

        throw new InvalidOperationException(
            "Unable to infer type of the template from its contents (stages, jobs, steps, variables). " +
            "Make sure the template contains at least one of these properties.");
    }

    private static string CreateMethodName(string templateName)
    {
        var name = Path.GetFileNameWithoutExtension(templateName);
        name = (name[0] + "").ToUpperInvariant() + name[1..];
        var rgx = MethodSanitize();
        return rgx.Replace(name, string.Empty);
    }

    private static string GetNewContent(string namespaceName, string className) =>
        $$"""
        using Sharpliner.AzureDevOps;

        namespace {{namespaceName}};

        public static class {{className}}
        {
        }
        """;

    private class ParsedTemplate
    {
        public Dictionary<string, object>? Parameters { get; set; }
        public List<Dictionary<object, object>>? Stages { get; set; }
        public List<Dictionary<object, object>>? Jobs { get; set; }
        public List<Dictionary<object, object>>? Steps { get; set; }
        public List<Dictionary<object, object>>? Variables { get; set; }
    }

    private class TemplateReferenceArgument
    {
        public string? Name { get; set; }
        public string? Type { get; set; }
        public object? Default { get; set; }
    }

    // We have to make sure that arguments with default values go first
    private class TemplateReferenceArgumentComparer : IComparer<TemplateReferenceArgument>
    {
        public static readonly TemplateReferenceArgumentComparer Instance = new();

        public int Compare(TemplateReferenceArgument? first, TemplateReferenceArgument? second)
        {
            if (first?.Default == null && second?.Default != null)
                return -1;
            if (first?.Default != null && second?.Default == null)
                return 1;
            else
                return 0;
        }
    }

    [GeneratedRegex("[^a-zA-Z0-9_]")]
    private static partial Regex MethodSanitize();
}
