using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Sharpliner.Common;

namespace Sharpliner;

/// <summary>
/// This is the main entrypoint that finds definitions via reflection and publishes YAMLs.
/// </summary>
public class SharplinerPublisher(TaskLoggingHelper logger)
{
    /// <summary>
    /// This method finds all pipeline definitions via reflection and publishes them to YAML.
    /// </summary>
    /// <param name="assemblyPath">Path to assembly with user's Sharpliner definitions</param>
    /// <param name="failIfChanged">
    ///     True to fail the task if there are new changes to be published.
    ///     This is for example used in the ValidateYamlsArePublished build step that checks that YAML changes were checked in.
    /// </param>
    public bool Publish(string assemblyPath, bool failIfChanged)
    {
        var definitionFound = false;
        var assembly = Assembly.Load(AssemblyName.GetAssemblyName(assemblyPath));

        LoadConfiguration(assembly);

        foreach (ISharplinerDefinition definition in FindAllImplementations<ISharplinerDefinition>(assembly))
        {
            definitionFound = true;
            PublishDefinition(definition, failIfChanged);
        }

        foreach ((ISharplinerDefinition definition, Type collection) in FindDefinitionsInCollections(assembly))
        {
            definitionFound = true;
            PublishDefinition(definition, failIfChanged, collection);
        }

        if (!definitionFound)
        {
            logger.LogMessage(MessageImportance.High, $"No definitions found in {assemblyPath}");
        }

        return true;
    }

    /// <summary>
    /// Publishes given ISharplinerDefinition into a YAML file.
    /// </summary>
    /// <param name="definition">ISharplinerDefinition object</param>
    /// <param name="failIfChanged">
    ///     True to fail the task if there are new changes to be published.
    ///     This is for example used in the ValidateYamlsArePublished build step that checks that YAML changes were checked in.
    /// </param>
    /// <param name="collection">Type of the collection the definition is coming from (if it is)</param>
    private void PublishDefinition(ISharplinerDefinition definition, bool failIfChanged, Type? collection = null)
    {
        var path = GetDestinationPath(definition);

        var typeName = collection == null ? definition.GetType().Name : collection.Name;

        logger.LogMessage(MessageImportance.High, $"{typeName} / {Path.GetFileName(path)}:");

        Validate(definition, typeName);

        string? hash = GetFileHash(path);

        Publish(definition);

        // Find out if there are new changes in the YAML
        if (hash == null)
        {
            if (failIfChanged)
            {
                logger.LogError($"  This definition hasn't been published yet!");
            }
            else
            {
                logger.LogMessage(MessageImportance.High, $"  {typeName} created at {path}");
            }
        }
        else
        {
            var newHash = GetFileHash(path);
            if (hash == newHash)
            {
                logger.LogMessage(MessageImportance.High, $"  No new changes to publish");
            }
            else
            {
                if (failIfChanged)
                {
                    logger.LogError($"  Changes detected between {typeName} and {path}");
                }
                else
                {
                    logger.LogMessage(MessageImportance.High, $"  Published new changes to {path}");
                }
            }
        }
    }

    /// <summary>
    /// Runs all validations configured on the definition
    /// </summary>
    /// <param name="definition">Definition</param>
    /// <param name="typeName">Type name, can include parent definition collection</param>
    private void Validate(ISharplinerDefinition definition, string typeName)
    {
        logger.LogMessage(MessageImportance.High, $"  Validating definition..");

        foreach (var validation in definition.Validations.Concat(SharplinerConfiguration.Current.RegisteredValidations))
        {
            var errors = validation.Validate();

            if (errors.Count > 0)
            {
                Log(errors.OrderByDescending(e => e.Severity).First().Severity,
                    $"  Validation of definition {typeName} failed!");
            }

            foreach (var error in errors)
            {
                Log(error.Severity, "    - " + error.Message);
            }
        }
    }

    private IEnumerable<(ISharplinerDefinition Definition, Type Collection)> FindDefinitionsInCollections(Assembly assembly) =>
        FindAllImplementations<ISharplinerDefinitionCollection>(assembly)
            .SelectMany(collection => collection.Definitions.Select(definition => (definition, collection.GetType())));

    private List<T> FindAllImplementations<T>(Assembly assembly)
    {
        var pipelines = new List<T>();
        var typeToFind = typeof(T);

        foreach (Type type in assembly.GetTypes().Where(t => t.IsClass && !t.IsAbstract && t.IsAssignableTo(typeToFind)))
        {
            object? pipelineDefinition = Activator.CreateInstance(type)
                ?? throw new Exception($"Failed to instantiate {type.GetType().FullName}");

            pipelines.Add((T)pipelineDefinition);
        }

        return pipelines;
    }

    private void LoadConfiguration(Assembly assembly)
    {
        var configurations = FindAllImplementations<SharplinerConfiguration>(assembly);

        if (configurations.Count > 1)
        {
            logger.LogWarning("Detected more than one Sharpliner configurations:"
                + Environment.NewLine + "  -"
                + string.Join(Environment.NewLine + "  -", configurations));
        }

        SharplinerConfiguration configuration = configurations.FirstOrDefault() ?? new DefaultSharplinerConfiguration();
        logger.LogMessage(MessageImportance.High, "Using {0} for configuration", configuration.GetType().Name);
        configuration.ConfigureInternal();
    }

    /// <summary>
    /// Gets the path where YAML of this definition should be published to
    /// </summary>
    private string GetDestinationPath(ISharplinerDefinition definition)
    {
        switch (definition.TargetPathType)
        {
            case TargetPathType.RelativeToGitRoot:
                var currentDir = new DirectoryInfo(Directory.GetCurrentDirectory());
                while (!Directory.Exists(Path.Combine(currentDir.FullName, ".git")))
                {
                    currentDir = currentDir.Parent;

                    if (currentDir == null)
                    {
                        throw new Exception($"Failed to find git repository in {Directory.GetParent(Assembly.GetExecutingAssembly().Location)?.FullName}");
                    }
                }

                return Path.Combine(currentDir.FullName, definition.TargetFile);

            case TargetPathType.RelativeToCurrentDir:
                return definition.TargetFile;

            case TargetPathType.RelativeToAssembly:
                return Path.Combine(Assembly.GetAssembly(definition.GetType())!.Location, definition.TargetFile);

            case TargetPathType.Absolute:
                return definition.TargetFile;

            default:
                throw new ArgumentException(nameof(definition.TargetPathType));
        }
    }

    /// <summary>
    /// Publishes the definition into a YAML file
    /// </summary>
    private void Publish(ISharplinerDefinition definition)
    {
        string destinationPath = GetDestinationPath(definition);

        SharplinerConfiguration.Current.Hooks.BeforePublish?.Invoke(definition, destinationPath);

        string yaml = definition.Serialize();

        if (SharplinerConfiguration.Current.Serialization.IncludeHeaders)
        {
            var header = definition.Header ?? GetDefaultHeader(GetType());

            if (header.Length > 0)
            {
                const string hash = "### ";
                yaml = hash + string.Join(Environment.NewLine + hash, header) + Environment.NewLine + Environment.NewLine + yaml;
                yaml = yaml.Replace(" \r\n", "\r\n").Replace(" \n", "\n"); // Remove trailing spaces from the default template
            }
        }

        var targetDirectory = Path.GetDirectoryName(destinationPath)!;
        if (!string.IsNullOrEmpty(targetDirectory) && !Directory.Exists(targetDirectory))
        {
            Directory.CreateDirectory(targetDirectory);
        }

        File.WriteAllText(destinationPath, yaml);

        SharplinerConfiguration.Current.Hooks.AfterPublish?.Invoke(definition, destinationPath, yaml);
    }

    private void Log(ValidationSeverity severity, string message)
    {
        switch (severity)
        {
            case ValidationSeverity.Trace:
                logger.LogMessage(message);
                break;
            case ValidationSeverity.Information:
                logger.LogMessage(MessageImportance.High, message);
                break;
            case ValidationSeverity.Warning:
                logger.LogWarning(message);
                break;
            case ValidationSeverity.Error:
                logger.LogError(message);
                break;
        }
    }

    /// <summary>
    /// Default YAML file header if one is not provided
    /// </summary>
    public static string[] GetDefaultHeader(Type type) =>
    [
        string.Empty,
        "DO NOT MODIFY THIS FILE!",
        string.Empty,
        $"This YAML was auto-generated from { type.Name }",
        $"To make changes, change the C# definition and rebuild its project",
        string.Empty,
    ];

    private static string? GetFileHash(string path)
    {
        if (!File.Exists(path))
        {
            return null;
        }

        using var md5 = MD5.Create();
        using var stream = File.OpenRead(path);
        return System.Text.Encoding.UTF8.GetString(md5.ComputeHash(stream));
    }
}
