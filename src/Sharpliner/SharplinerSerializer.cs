using System;
using System.Text.RegularExpressions;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Sharpliner;

#if NET7_0_OR_GREATER
public static partial class SharplinerSerializer
#else
public static class SharplinerSerializer
#endif
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
        var newLineReplace = Environment.NewLine + "$1";
        yaml = SectionStartRegex().Replace(yaml, newLineReplace);
        yaml = MainItemStartRegex().Replace(yaml, newLineReplace);
        yaml = ConditionedBlockStartRegex().Replace(yaml, newLineReplace);
        yaml = DoubleNewLineStartRegex().Replace(yaml, ":" + Environment.NewLine);
        return yaml;
    }

#if NET7_0_OR_GREATER
    [GeneratedRegex("((\r?\n)[a-zA-Z]+:)", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex SectionStartRegex();

    [GeneratedRegex("((\r?\n) {0,8}- ?[a-zA-Z]+@?[a-zA-Z\\.0-9]*:)", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex MainItemStartRegex();

    [GeneratedRegex("((\r?\n) {0,8}- ?\\${{ ?(if|else|each)[^\n]+\n)", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex ConditionedBlockStartRegex();

    [GeneratedRegex("(:\r?\n\r?\n)", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex DoubleNewLineStartRegex();
#else
    private static readonly Regex s_sectionStartRegex = new("((\r?\n)[a-zA-Z]+:)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static Regex SectionStartRegex() => s_sectionStartRegex;
    
    private static readonly Regex s_mainItemStartRegex = new("((\r?\n) {0,8}- ?[a-zA-Z]+@?[a-zA-Z\\.0-9]*:)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static Regex MainItemStartRegex() => s_mainItemStartRegex;
    
    private static readonly Regex s_conditionedBlockStartRegex = new("((\r?\n) {0,8}- ?\\${{ ?(if|else|each)[^\n]+\n)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static Regex ConditionedBlockStartRegex() => s_conditionedBlockStartRegex;
    
    private static readonly Regex s_doubleNewLineStartRegex = new("(:\r?\n\r?\n)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static Regex DoubleNewLineStartRegex() => s_doubleNewLineStartRegex;
#endif
}
