using System.Collections.Generic;

namespace Sharpliner.Tools.TemplateApi.Model;

// There are two main ways to define parameters
// The first way is that parameters are fully specified
//   - name: myBoolean
//     type: boolean
//     default: true
//
// The second way is that parameters are key:value pairs of name:defaultValue
//   myBoolean: true
//
internal abstract class ParsedTemplate
{
    public List<Dictionary<object, object>>? Stages { get; set; }
    public List<Dictionary<object, object>>? Jobs { get; set; }
    public List<Dictionary<object, object>>? Steps { get; set; }
    public List<Dictionary<object, object>>? Variables { get; set; }
}

internal class FullySpecifiedTemplate : ParsedTemplate
{
    public List<Dictionary<object, object>>? Parameters { get; set; }
}

internal class OnlyDefaultValuesTemplate : ParsedTemplate
{
    public Dictionary<string, object>? Parameters { get; set; }
}
