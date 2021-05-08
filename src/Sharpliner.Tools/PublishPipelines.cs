using System.Collections.Generic;
using System;
using Microsoft.Build.Framework;
using System.Linq;
using Sharpliner.AzureDevOps;
using System.Reflection;

namespace Sharpliner.Tools
{
    public class PublishPipelines : Microsoft.Build.Utilities.Task
    {
        /// <summary>
        /// Assembly that will be scaned for pipeline definitions.
        /// </summary>
        [Required]
        public string? Assembly { get; set; }

        public override bool Execute()
        {
            var definitions = FindPipelines<AzureDevOpsPipelineDefinition>();

            foreach (var definition in definitions)
            {
                Log.LogMessage(MessageImportance.High, "Found " + definition.TargetFile);
            }

            return true;
        }

        private IEnumerable<T> FindPipelines<T>() where T : class
        {
            var objects = new List<T>();
            var assembly = System.Reflection.Assembly.LoadFile(Assembly ?? throw new Exception("Failed to read current assembly name"));

            Log.LogMessage(MessageImportance.High, "Looking for " + typeof(T).UnderlyingSystemType.GUID);

            foreach (Type type in assembly.GetTypes())
            {
                if (type.BaseType?.GUID != typeof(T).GUID)
                {
                    continue;
                }

                Log.LogMessage(MessageImportance.High, "Found " + type.FullName);

                //if (type.BaseType != typeof(T))
                //{
                //    Log.LogError("NOOO");
                //}

                if (type.BaseType.IsAssignableFrom(typeof(T)) || type.BaseType.IsAssignableTo(typeof(T)) || type.IsSubclassOf(typeof(T)))
                {
                    Log.LogMessage(MessageImportance.High, "YEEEEES " + type.FullName);

                    var foo = Activator.CreateInstance(type);

                    Log.LogMessage(MessageImportance.High, "Instantiated " + foo?.GetType());
                    if (foo is not T obj)
                    {
                        throw new Exception($"Failed to instantiate {type.GetType().FullName}");
                    }

                    objects.Add(obj);
                }
                else
                {
                    Log.LogError($"Failed to assign {Environment.NewLine}{type.BaseType.FullName}{Environment.NewLine}{typeof(T).FullName}" +
                        $"{Environment.NewLine}{type.BaseType.GUID}{Environment.NewLine}{typeof(T).GUID}");
                }
            }

            /*foreach (Type type in assembly.GetTypes().Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(T))))
            {
                Log.LogMessage(MessageImportance.High, "Found " + type.FullName);

                if (Activator.CreateInstance(type) is not T obj)
                {
                    throw new Exception($"Failed to instantiate {type.GetType().FullName}");
                }

                objects.Add(obj);
            }*/

            objects.Sort();
            return objects;
        } 
    }
}
