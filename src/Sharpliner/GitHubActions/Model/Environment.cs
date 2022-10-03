using System;

namespace Sharpliner.GitHubActions;

public record Environment
{
    public Environment(string name, string? url = null)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentNullException(nameof(name));
        }

        Name = name;
        Url = url;
    }

    public string Name { get; init; }
    public string? Url { get; init; }
}
