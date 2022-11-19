using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Sharpliner.Tools.TemplateApi.Model;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Sharpliner.Tools.TemplateApi;

public class TemplateApiGenerator
{
    private string _newLine = Environment.NewLine;

    private static readonly IDeserializer s_deserializer = new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .Build();

    public List<string> AddOrUpdateApi(string namespaceName, string className, string? content, string templatePath)
    {
        // We try to parse the template file twice as there are two main ways to define parameters
        // The first way is that parameters are fully specified
        //   - name: myBoolean
        //     type: boolean
        //     default: true
        //
        // The second way is that parameters are key:value pairs of name:defaultValue
        //   myBoolean: true
        //
        ParsedTemplate template;
        try
        {
            using (var reader = File.OpenText(templatePath))
            {
                template = s_deserializer.Deserialize<OnlyDefaultValuesTemplate>(reader);
            }
        }
        catch (YamlException e) when (e.Message.StartsWith("Expected 'MappingStart', got 'SequenceStart'"))
        {
            using (var reader = File.OpenText(templatePath))
            {
                template = s_deserializer.Deserialize<FullySpecifiedTemplate>(reader);
            }
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

    private List<string> CreateMethod(string relativePath, string methodName, string type, TemplateParameterDefinition[] arguments)
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

    private static TemplateParameterDefinition[] GetArguments(ParsedTemplate parsedTemplate)
    {
        IEnumerable<TemplateParameterDefinition>? parameters = parsedTemplate switch
        {
            FullySpecifiedTemplate fullySpecifiedTemplate => fullySpecifiedTemplate.Parameters?.Select(ParseFullParameterDefinition),
            OnlyDefaultValuesTemplate defaultValuesTemplate => defaultValuesTemplate.Parameters?.Select(pair =>
            {
                var parameter = new TemplateParameterDefinition
                {
                    Name = pair.Key,
                };

                (parameter.Type, parameter.Default) = ParseArgumentTypeAndDefault(pair.Value);
                return parameter;
            }),
            _ => throw new NotImplementedException(),
        };

        if (parameters == null)
        {
            return Array.Empty<TemplateParameterDefinition>();
        }

        return parameters
            .Order(TemplateParameterDefinitionComparer.Instance)
            .ToArray();
    }

    private static (string, object?) ParseArgumentTypeAndDefault(object value) => value switch
    {
        Dictionary<object, object> => ("TaskInputs?", "null"),
        int i => ("int", i),
        double d => ("double", d),
        float f => ("float", f),
        "true" or "false" => ("bool", ((string)value).ToLowerInvariant()),
        string s => ("string", s is not null ? $"\"{s}\"" : null),
        bool b => ("bool", b),
        IEnumerable<object> => ("List<MISSING_TYPE>", null),
        _ => ("MISSING_TYPE", null),
    };

    private static TemplateParameterDefinition ParseFullParameterDefinition(Dictionary<object, object> argument)
    {
        var parameter = new TemplateParameterDefinition();

        if (argument.TryGetValue("name", out var nameDefinition))
        {
            parameter.Name = nameDefinition?.ToString();
        }

        if (string.IsNullOrEmpty(parameter.Name))
        {
            throw new Exception("Expected 'name' on parameter definition");
        }

        if (argument.TryGetValue("type", out var typeName))
        {
            if (argument.TryGetValue("default", out var definedDefault))
            {
                parameter.Default = ParseArgumentTypeAndDefault(definedDefault).Item2;
            }

            parameter.Type = typeName switch
            {
                "string" => "string",
                "number" => "int",
                "boolean" => "bool",
                "object" => "TaskInputs?",
                "step" => "Conditioned<Step>",
                "stepList" => "ConditionedList<Step>",
                "job" => "Conditioned<JobBase>",
                "jobList" => "ConditionedList<JobBase>",
                "deployment" => "Conditioned<DeploymentJob>",
                "deploymentList" => "ConditionedList<DeploymentJob>",
                "stage" => "Conditioned<Stage>",
                "stageList" => "ConditionedList<Stage>",
                _ => "TaskInputs?",
            };

            if (typeName?.ToString() == "number")
            {
                parameter.Default = (parameter.Default as string)?.Replace("\"", null);
            }
        }
        else if (argument.TryGetValue("default", out var definedDefault))
        {
            (parameter.Type, parameter.Default) = ParseArgumentTypeAndDefault(definedDefault);
        }

        return parameter;
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
        var nameBuild = new StringBuilder();

        for (int i = 0; i < name.Length; i++)
        {
            if (!char.IsLetter(name[i]) && !char.IsDigit(name[i]))
            {
                continue;
            }

            if (i == 0 || (!char.IsLetter(name[i - 1]) && !char.IsDigit(name[i - 1])))
            {
                nameBuild.Append(char.ToUpper(name[i]));
            }
            else
            {
                nameBuild.Append(name[i]);
            }
        }

        return nameBuild.ToString();
    }

    private static string GetNewContent(string namespaceName, string className) =>
        $$"""
        using System.Collections.Generic;
        using Sharpliner.AzureDevOps;
        using Sharpliner.AzureDevOps.ConditionedExpressions;
        using Sharpliner.AzureDevOps.Tasks;

        namespace {{namespaceName}};

        public static class {{className}}
        {
        }
        """;
}
