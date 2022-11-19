using System.Collections.Generic;

namespace Sharpliner.Tools.TemplateApi.Model;

// We have to make sure that arguments with default values go first
internal class TemplateParameterDefinitionComparer : IComparer<TemplateParameterDefinition>
{
    public static readonly TemplateParameterDefinitionComparer Instance = new();

    private TemplateParameterDefinitionComparer() { }

    public int Compare(TemplateParameterDefinition? first, TemplateParameterDefinition? second)
    {
        if (first?.Default == null && second?.Default != null)
            return -1;
        if (first?.Default != null && second?.Default == null)
            return 1;
        else
            return 0;
    }
}
