namespace Sharpliner.Tools;

/// <summary>
/// Exit codes to use for common failure reasons; if you add a new exit code, add it here and use the enum.
/// The first part conforms with xUnit: https://xunit.net/docs/getting-started/netfx/visual-studio
/// </summary>
public enum ExitCode
{
    /// <summary>
    /// The tests ran successfully
    /// </summary>
    SUCCESS = 0,

    /// <summary>
    /// The help page was shown
    /// Either because it was requested, or because the user did not provide any command line arguments
    /// </summary>
    HELP_SHOWN = 2,

    /// <summary>
    /// There was a problem with one of the command line options
    /// </summary>
    INVALID_ARGUMENTS = 3,

    /// <summary>
    /// Generic code for cases where we couldn't determine the exact cause
    /// </summary>
    GENERAL_FAILURE = 71,
}
