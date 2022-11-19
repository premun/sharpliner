using System;
using System.Text.RegularExpressions;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Sharpliner;

public static class SharplinerSerializer
{
    public static ISerializer Serializer { get; } = InitializeSerializer();

    public static string Serialize(object data)
    {
        var yaml = Serializer.Serialize(data);
        return SharplinerConfiguration.Current.Serialization.PrettifyYaml ? Prettify(yaml) : yaml;
    }

    private static ISerializer InitializeSerializer()
    {
        var serializerBuilder = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitDefaults | DefaultValuesHandling.OmitEmptyCollections)
            .WithEventEmitter(nextEmitter => new MultilineStringEmitter(nextEmitter));

        return serializerBuilder.Build();
    }

    public static string Prettify(string yaml)
    {
        // Add empty new lines to make text more readable
        yaml = Regex.Replace(yaml, "((\r?\n)[a-zA-Z]+:)", Environment.NewLine + "$1");
        yaml = Regex.Replace(yaml, "((\r?\n) {0,8}- ?[a-zA-Z]+@?[a-zA-Z\\.0-9]*:)", Environment.NewLine + "$1");
        yaml = Regex.Replace(yaml, "((\r?\n) {0,8}- ?\\${{ ?if[^\n]+\n)", Environment.NewLine + "$1");
        yaml = Regex.Replace(yaml, "((\r?\n) {0,8}- ?\\${{ ?else[^\n]+\n)", Environment.NewLine + "$1");
        yaml = Regex.Replace(yaml, "(:\r?\n\r?\n)", ":" + Environment.NewLine);

        return yaml;
    }
}
