using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Microsoft.Build.Framework;

namespace Sharpliner;

/// <summary>
/// This is an MSBuild task that is run in user projects to publish YAMLs after build.
/// </summary>
public class PublishDefinitions : Microsoft.Build.Utilities.Task
{
    /// <summary>
    /// Assembly that will be scaned for pipeline definitions.
    /// </summary>
    [Required]
    public string? Assembly { get; set; }

    /// <summary>
    /// You can make the task fail in case it finds a YAML whose definition changed.
    /// This is for example used in the ValidateYamlsArePublished build step that checks that YAML changes were checked in.
    /// </summary>
    public bool FailIfChanged { get; set; }

    /// <summary>
    /// This method finds all pipeline definitions via reflection and publishes them to YAML.
    /// </summary>
    public override bool Execute()
    {
        if (string.IsNullOrEmpty(Assembly))
        {
            throw new ArgumentNullException(nameof(Assembly), "Assembly parameter not set");
        }

        if (!File.Exists(Assembly))
        {
            throw new FileNotFoundException(Assembly);
        }

        // We need to copy the DLL into our application's path so that Assembly.Load can resolve it properly and add to the main binding context
        var dest = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase)!, Path.GetFileName(Assembly)!)
            .Replace("file:\\", "");

        Log.LogWarning($"Copying {Assembly} to {dest}");
        File.Copy(Assembly!, dest, true);

        Assembly assembly = System.Reflection.Assembly.Load(AssemblyName.GetAssemblyName(Assembly));

        var definitionFound = false;

        foreach (ISharplinerDefinition definition in FindAllImplementations<ISharplinerDefinition>(assembly))
        {
            definitionFound = true;
            PublishDefinition(definition);
        }

        foreach ((ISharplinerDefinition definition, Type collection) in FindDefinitionsInCollections(assembly))
        {
            definitionFound = true;
            PublishDefinition(definition, collection);
        }

        if (!definitionFound)
        {
            Log.LogMessage(MessageImportance.High, $"No definitions found in {Assembly}");
        }

        return true;
    }

    /// <summary>
    /// Publishes given ISharplinerDefinition into a YAML file.
    /// </summary>
    /// <param name="definition">ISharplinerDefinition object</param>
    /// <param name="collection">Type of the collection the definition is coming from (if it is)</param>
    private void PublishDefinition(ISharplinerDefinition definition, Type? collection = null)
    {
        var path = definition.GetTargetPath();

        var typeName = collection == null ? definition.GetType().Name : collection.Name + " / " + Path.GetFileName(path);

        Log.LogMessage(MessageImportance.High, $"{typeName}:");
        Log.LogMessage(MessageImportance.High, $"  Validating definition..");

        try
        {
            definition.Validate();
        }
        catch (TargetInvocationException e)
        {
            Log.LogError("Validation of definition {0} failed: {1}{2}{3}",
                typeName,
                e.InnerException?.Message ?? e.ToString(),
                Environment.NewLine,
                "To see exception details, build with more verbosity (dotnet build -v:n)");

            Log.LogMessage(MessageImportance.Normal, "Validation of definition {0} failed: {1}", typeName, e.InnerException);
            return;
        }

        string? hash = GetFileHash(path);

        // Publish pipeline
        definition.Publish();

        if (hash == null)
        {
            if (FailIfChanged)
            {
                Log.LogError($"  This definition hasn't been published yet!");
            }
            else
            {
                Log.LogMessage(MessageImportance.High, $"  {typeName} created at {path}");
            }
        }
        else
        {
            var newHash = GetFileHash(path);
            if (hash == newHash)
            {
                Log.LogMessage(MessageImportance.High, $"  No new changes to publish");
            }
            else
            {
                if (FailIfChanged)
                {
                    Log.LogError($"  Changes detected between {typeName} and {path}");
                }
                else
                {
                    Log.LogMessage(MessageImportance.High, $"  Published new changes to {path}");
                }
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
            object? pipelineDefinition = Activator.CreateInstance(type);
            if (pipelineDefinition is null)
            {
                throw new Exception($"Failed to instantiate {type.GetType().FullName}");
            }

            pipelines.Add((T)pipelineDefinition);
        }

        return pipelines;
    }

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
