using System;
using System.Collections.Generic;

namespace Sharpliner.GitHubActions;

public enum GitHubPermissionScope
{
    Actions,
    Checks,
    Contents,
    Deployments,
    Issues,
    Packages,
    PullRequests,
    RepositoryProjects,
    SecurityEvents,
    Statuses,
}

public enum GitHubPermission
{
    None,
    Read,
    Write
}

public record Permissions
{
    public HashSet<GitHubPermissionScope> Read { get; } = new();
    public HashSet<GitHubPermissionScope> Write { get; } = new();

    public Permissions All(GitHubPermission permission)
    {
        // loop and add, we are not worried because it is a hasset, we add
        // to the correct hashset or remove from both in the case of None
        foreach (var scope in Enum.GetValues<GitHubPermissionScope>())
        {
            switch (permission)
            {
                case GitHubPermission.Read:
                    Read.Add(scope);
                    break;
                case GitHubPermission.Write:
                    Write.Add(scope);
                    break;
                default:
                    Read.Remove(scope);
                    Write.Remove(scope);
                    break;
            }
        }
        return this;
    }
}
