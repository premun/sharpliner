using System.ComponentModel;
using YamlDotNet.Serialization;

namespace Sharpliner.AzureDevOps.Tasks;

/// <summary>
/// More details can be found in <see href="https://docs.microsoft.com/en-us/azure/devops/pipelines/yaml-schema?view=azure-devops&amp;tabs=schema%2Cparameter-schema#checkout">official Azure DevOps pipelines documentation</see>.
/// </summary>
public abstract record CheckoutTask : Step
{
    [YamlMember(Order = 1)]
    public abstract string Checkout { get; }

    /// <summary>
    /// When true, run `execute git clean -ffdx &amp;&amp; git reset --hard HEAD` before fetching.
    /// Defaults to false.
    /// </summary>
    [YamlMember(Order = 100)]
    [DefaultValue(false)]
    public bool Clean { get; init; } = false;

    /// <summary>
    /// The depth of commits to ask Git to fetch.
    /// Defaults to shallow fetch (= 1).
    /// Set 0 to no limit (full clone).
    /// </summary>
    [YamlMember(Order = 101)]
    [DefaultValue(1)]
    public int FetchDepth { get; init; } = 1;

    /// <summary>
    /// Whether to download Git-LFS files.
    /// Defaults to false.
    /// </summary>
    [YamlMember(Order = 102)]
    [DefaultValue(false)]
    public bool Lfs { get; init; } = false;

    /// <summary>
    /// Submodules checkout strategy (single level, recursive to get submodules of submodules).
    /// Defaults to not checking out submodules.
    /// </summary>
    [YamlIgnore]
    [DefaultValue(SubmoduleCheckout.None)]
    public SubmoduleCheckout Submodules { get; init; } = SubmoduleCheckout.None;

    [YamlMember(Alias = "submodules", Order = 103)]
    [DefaultValue("false")]
    public string _Submodules => Submodules switch
    {
        SubmoduleCheckout.SingleLevel => "true",
        SubmoduleCheckout.Recursive => "recursive",
        _ => "false",
    };

    /// <summary>
    /// Path to check out source code, relative to the agent's build directory (e.g. \_work\1).
    /// Defaults to a directory called `s`.
    /// </summary>
    [YamlMember(Order = 104)]
    [DefaultValue("s")]
    public string Path { get; init; } = "s";

    /// <summary>
    /// When true, leave the OAuth token in the Git config after the initial fetch.
    /// Defaults to false.
    /// </summary>
    [YamlMember(Order = 105)]
    [DefaultValue(false)]
    public bool PersistCredentials { get; init; } = false;
}

public record SelfCheckoutTask : CheckoutTask
{
    public override string Checkout => "self";
}

public record NoneCheckoutTask : Step
{
    [YamlMember(Order = 1)]
    public string Checkout => "none";
}

public record RepositoryCheckoutTask : CheckoutTask
{
    private readonly string _repository;

    public override string Checkout => _repository;

    public RepositoryCheckoutTask(string repository)
    {
        _repository = repository;
    }
}

public enum SubmoduleCheckout
{
    None,
    SingleLevel,
    Recursive,
}
