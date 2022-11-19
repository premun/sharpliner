using Sharpliner.AzureDevOps.ConditionedExpressions;

namespace Sharpliner.AzureDevOps;

public class VariablesReference
{
    public VariableReference this[string variableName] => new(variableName);

    /// <summary>
    /// Variables connected to the agent running the current build (e.g. Agent.HomeDirectory)
    /// More details can be found in <see href="https://docs.microsoft.com/en-us/azure/devops/pipelines/build/variables?view=azure-devops&amp;tabs=yaml#agent-variables-devops-services">official Azure DevOps pipelines documentation</see>.
    /// </summary>
    public AgentVariableReference Agent { get; } = new();

    /// <summary>
    /// Variables connected to the current build (e.g. build number)
    /// More details can be found in <see href="https://docs.microsoft.com/en-us/azure/devops/pipelines/build/variables?view=azure-devops&amp;tabs=yaml#build-variables-devops-services">official Azure DevOps pipelines documentation</see>.
    /// </summary>
    public BuildVariableReference Build { get; } = new();

    /// <summary>
    /// System variables (e.g. System.AccessToken or System.Debug)
    /// </summary>
    public SystemVariableReference System { get; } = new();

    /// <summary>
    /// Variables connected to the pipeline (not build)
    /// </summary>
    public PipelineVariableReference Pipeline { get; } = new();

    /// <summary>
    /// Deployment variables
    /// These variables are scoped to a specific Deployment job and will be resolved only at job execution time
    /// </summary>
    public EnvironmentVariableReference Environment { get; } = new();

    /// <summary>
    /// Deployment variables
    /// These variables are scoped to a specific Deployment job and will be resolved only at job execution time
    /// </summary>
    public StrategyVariableReference Strategy { get; } = new();

    /// <summary>
    /// Set to True if the script is being run by a build task.
    /// This variable is agent-scoped, and can be used as an environment variable in a script and as a parameter in a build task, but not as part of the build number or as a version control tag.
    /// </summary>
    public VariableReference TF_BUILD => VariableReferenceBase.GetReference(string.Empty, "TF_BUILD");

    public VariableReference Configuration => VariableReferenceBase.GetReference(string.Empty, "Configuration");
}

public abstract class VariableReferenceBase
{
    internal static VariableReference GetReference(string prefix, string variableName) => new($"{prefix}{variableName}");

    public VariableReference this[string variableName] => GetReference(Prefix, variableName);

    protected VariableReference GetReference(string name) => GetReference(Prefix, name);

    protected abstract string Prefix { get; }
}

public sealed class AgentVariableReference : VariableReferenceBase
{
    protected override string Prefix => "Agent.";

    internal AgentVariableReference()
    {
    }

    /// <summary>
    /// The local path on the agent where all folders for a given build pipeline are created.
    /// This variable has the same value as Pipeline.Workspace.
    /// For example: /home/vsts/work/1
    /// </summary>
    public VariableReference BuildDirectory => GetReference("BuildDirectory");

    /// <summary>
    /// A mapping from container resource names in YAML to their Docker IDs at runtime.
    /// For example:
    /// {
    ///   "one_container": {
    ///     "id": "bdbb357d73a0bd3550a1a5b778b62a4c88ed2051c7802a0659f1ff6e76910190"
    ///   },
    ///   "another_container": {
    ///     "id": "82652975109ec494876a8ccbb875459c945982952e0a72ad74c91216707162bb"
    ///   }
    /// }
    /// </summary>
    public VariableReference ContainerMapping => GetReference("ContainerMapping");

    /// <summary>
    /// The directory the agent is installed into. This contains the agent software.
    /// For example: c:\agent.
    /// </summary>
    public VariableReference HomeDirectory => GetReference("HomeDirectory");

    /// <summary>
    /// The ID of the agent.
    /// </summary>
    public VariableReference Id => GetReference("Id");

    /// <summary>
    /// The name of the running job.
    /// This will usually be "Job" or "__default", but in multi-config scenarios, will be the configuration.
    /// </summary>
    public VariableReference JobName => GetReference("JobName");

    /// <summary>
    /// The status of the build.
    ///   - Canceled
    ///   - Failed
    ///   - Succeeded
    ///   - SucceededWithIssues (partially successful)
    /// </summary>
    public VariableReference JobStatus => GetReference("JobStatus");

    /// <summary>
    /// The name of the machine on which the agent is installed.
    /// </summary>
    public VariableReference MachineName => GetReference("MachineName");

    /// <summary>
    /// The name of the agent that is registered with the pool.
    /// If you are using a self-hosted agent, then this name is specified by you. See agents.
    /// </summary>
    public VariableReference Name => GetReference("Name");

    /// <summary>
    /// The operating system of the agent host.
    /// Valid values are:
    ///   - Windows_NT
    ///   - Darwin
    ///   - Linux
    /// </summary>
    public VariableReference OS => GetReference("OS");

    /// <summary>
    /// The operating system processor architecture of the agent host.
    /// Valid values are:
    ///   - X86
    ///   - X64
    ///   - ARM
    /// </summary>
    public VariableReference OSArchitecture => GetReference("OSArchitecture");

    /// <summary>
    /// A temporary folder that is cleaned after each pipeline job.
    /// This directory is used by tasks such as .NET Core CLI task to hold temporary items like test results before they are published.
    /// For example: /home/vsts/work/_temp for Ubuntu
    /// </summary>
    public VariableReference TempDirectory => GetReference("TempDirectory");

    /// <summary>
    /// The directory used by tasks such as Node Tool Installer and Use Python Version to switch between multiple versions of a tool.
    /// These tasks will add tools from this directory to PATH so that subsequent build steps can use them.
    /// Learn about managing this directory on a self-hosted agent.
    /// </summary>
    public VariableReference ToolsDirectory => GetReference("ToolsDirectory");

    /// <summary>
    /// The working directory for this agent.
    /// For example: c:\agent_work
    /// Note: This directory is not guaranteed to be writable by pipeline tasks (eg. when mapped into a container)
    /// </summary>
    public VariableReference WorkFolder => GetReference("WorkFolder");
}

public sealed class BuildVariableReference : VariableReferenceBase
{
    internal static readonly BuildVariableReference Instance = new();

    protected override string Prefix => "Build.";

    internal BuildVariableReference()
    {
    }

    /// <summary>
    /// The local path on the agent where any artifacts are copied to before being pushed to their destination.
    /// For example: c:\agent_work\1\a
    ///
    /// A typical way to use this folder is to publish your build artifacts with the Copy files and Publish build artifacts tasks.
    ///
    /// Note: Build.ArtifactStagingDirectory and Build.StagingDirectory are interchangeable. This directory is purged before each new build, so you don't have to clean it up yourself.
    ///
    /// See Artifacts in Azure Pipelines.
    ///
    /// This variable is agent-scoped, and can be used as an environment variable in a script and as a parameter in a build task, but not as part of the build number or as a version control tag.
    /// </summary>
    public VariableReference ArtifactStagingDirectory => GetReference("ArtifactStagingDirectory");

    /// <summary>
    /// The local path on the agent where any artifacts are copied to before being pushed to their destination.
    /// For example: c:\agent_work\1\a
    /// A typical way to use this folder is to publish your build artifacts with the Copy files and Publish build artifacts tasks.
    /// Note: Build.ArtifactStagingDirectory and Build.StagingDirectory are interchangeable. This directory is purged before each new build, so you don't have to clean it up yourself.
    /// See Artifacts in Azure Pipelines.
    /// This variable is agent-scoped, and can be used as an environment variable in a script and as a parameter in a build task, but not as part of the build number or as a version control tag.
    /// </summary>
    public VariableReference StagingDirectory => GetReference("StagingDirectory");

    /// <summary>
    /// The ID of the record for the completed build.
    /// </summary>
    public VariableReference BuildId => GetReference("BuildId");

    /// <summary>
    /// The name of the completed build, also known as the run number.You can specify what is included in this value.
    /// A typical use of this variable is to make it part of the label format, which you specify on the repository tab.
    ///
    /// Note: This value can contain whitespace or other invalid label characters. In these cases, the label format will fail.
    ///
    /// This variable is agent-scoped, and can be used as an environment variable in a script and as a parameter in a build task, but not as part of the build number or as a version control tag.
    /// </summary>
    public VariableReference BuildNumber => GetReference("BuildNumber");

    /// <summary>
    /// The URI for the build.
    /// For example: vstfs:///Build/Build/1430
    ///
    /// This variable is agent-scoped, and can be used as an environment variable in a script and as a parameter in a build task, but not as part of the build number or as a version control tag.
    /// </summary>
    public VariableReference BuildUri => GetReference("BuildUri");

    /// <summary>
    /// The local path on the agent you can use as an output folder for compiled binaries.
    /// By default, new build pipelines are not set up to clean this directory. You can define your build to clean it up on the Repository tab.
    /// For example: c:\agent_work\1\b
    ///
    /// This variable is agent-scoped, and can be used as an environment variable in a script and as a parameter in a build task, but not as part of the build number or as a version control tag.
    /// </summary>
    public VariableReference BinariesDirectory => GetReference("BinariesDirectory");

    /// <summary>
    /// The ID of the container for your artifact.
    /// When you upload an artifact in your pipeline, it is added to a container that is specific for that particular artifact.
    /// </summary>
    public VariableReference ContainerId => GetReference("ContainerId");

    /// <summary>
    /// The name of the build pipeline.
    /// Note: This value can contain whitespace or other invalid label characters. In these cases, the label format will fail.
    /// </summary>
    public VariableReference DefinitionName => GetReference("DefinitionName");

    /// <summary>
    /// The version of the build pipeline.
    /// </summary>
    public VariableReference DefinitionVersion => GetReference("DefinitionVersion");

    /// <summary>
    /// See "How are the identity variables set?".
    /// Note: This value can contain whitespace or other invalid label characters. In these cases, the label format will fail.
    /// </summary>
    public VariableReference QueuedBy => GetReference("QueuedBy");

    /// <summary>
    /// See "How are the identity variables set?"
    /// </summary>
    public VariableReference QueuedById => GetReference("QueuedById");

    /// <summary>
    /// The event that caused the build to run.
    ///   - Manual: A user manually queued the build.
    ///   - IndividualCI: Continuous integration (CI) triggered by a Git push or a TFVC check-in.
    ///   - BatchedCI: Continuous integration (CI) triggered by a Git push or a TFVC check-in, and the Batch changes was selected.
    ///   - Schedule: Scheduled trigger.
    ///   - ValidateShelveset: A user manually queued the build of a specific TFVC shelveset.
    ///   - CheckInShelveset: Gated check-in trigger.
    ///   - PullRequest: The build was triggered by a Git branch policy that requires a build.
    ///   - ResourceTrigger: The build was triggered by a resource trigger or it was triggered by another build.
    /// </summary>
    public VariableReference Reason => GetReference("Reason");

    /// <summary>
    /// Variables connected to repository information
    /// </summary>
    public RepositoryVariableReference Repository => new();

    /// <summary>
    /// See "How are the identity variables set?"
    /// Note: This value can contain whitespace or other invalid label characters. In these cases, the label format will fail.
    /// </summary>
    public VariableReference RequestedFor => GetReference("RequestedFor");

    /// <summary>
    /// See "How are the identity variables set?"
    /// </summary>
    public VariableReference RequestedForEmail => GetReference("RequestedForEmail");

    /// <summary>
    /// See "How are the identity variables set?"
    /// </summary>
    public VariableReference RequestedForId => GetReference("RequestedForId");

    /// <summary>
    /// The branch of the triggering repo the build was queued for.
    /// Some examples:
    ///   - Git repo branch: refs/heads/master
    ///   - Git repo pull request: refs/pull/1/merge
    ///   - TFVC repo branch: $/teamproject/main
    ///   - TFVC repo gated check-in: Gated_2016-06-06_05.20.51.4369;username @live.com
    ///   - TFVC repo shelveset build: myshelveset; username @live.com
    ///   - When your pipeline is triggered by a tag: refs/tags/your-tag-name
    ///
    /// When you use this variable in your build number format, the forward slash characters (/) are replaced with underscore characters _).
    /// Note: In TFVC, if you are running a gated check-in build or manually building a shelveset, you cannot use this variable in your build number format.
    /// </summary>
    public VariableReference SourceBranch => GetReference("SourceBranch");

    /// <summary>
    /// The name of the branch in the triggering repo the build was queued for.
    ///   - Git repo branch or pull request: The last path segment in the ref. For example: in refs/heads/master this value is master.In refs/heads/feature/tools this value is tools.
    ///   - TFVC repo branch: The last path segment in the root server path for the workspace. For example: in $/teamproject/main this value is main.
    ///   - TFVC repo gated check-in or shelveset build is the name of the shelveset.For example: Gated_2016-06-06_05.20.51.4369; username @live.com or myshelveset; username @live.com.
    ///
    /// Note: In TFVC, if you are running a gated check-in build or manually building a shelveset, you cannot use this variable in your build number format. 	Yes
    /// </summary>
    public VariableReference SourceBranchName => GetReference("SourceBranchName");

    /// <summary>
    /// The local path on the agent where your source code files are downloaded. For example: c:\agent_work\1\s
    /// By default, new build pipelines update only the changed files.
    ///
    /// Important note: If you check out only one Git repository, this path will be the exact path to the code. If you check out multiple repositories, it will revert to its default value, which is $(Pipeline.Workspace)/s, even if the self (primary) repository is checked out to a custom path different from its multi-checkout default path $(Pipeline.Workspace)/s/[RepoName] (in this respect, the variable differs from the behavior of the Build.Repository.LocalPath variable).
    ///
    /// This variable is agent-scoped, and can be used as an environment variable in a script and as a parameter in a build task, but not as part of the build number or as a version control tag.
    /// </summary>
    public VariableReference SourcesDirectory => GetReference("SourcesDirectory");

    /// <summary>
    /// The latest version control change of the triggering repo that is included in this build.
    ///   - Git: The commit ID
    ///   - TFVC: the changeset
    /// </summary>
    public VariableReference SourceVersion => GetReference("SourceVersion");

    /// <summary>
    /// The comment of the commit or changeset for the triggering repo. We truncate the message to the first line or 200 characters, whichever is shorter.
    /// The Build.SourceVersionMessage corresponds to the message on Build.SourceVersion commit. The Build.SourceVersion commit for a PR build is the merge commit (not the commit on the source branch).
    /// This variable is agent-scoped, and can be used as an environment variable in a script and as a parameter in a build task, but not as part of the build number or as a version control tag. Also, this variable is only available on the step level and is neither available in the job nor stage levels (i.e. the message is not extracted until the job had started and checked out the code).
    /// Note: This variable is available in TFS 2015.4.
    /// </summary>
    public VariableReference SourceVersionMessage => GetReference("SourceVersionMessage");

    /// <summary>
    /// Defined if your repository is Team Foundation Version Control.
    /// If you are running a gated build or a shelveset build, this is set to the name of the shelveset you are building.
    /// Note: This variable yields a value that is invalid for build use in a build number format.
    /// </summary>
    public VariableReference SourceTfvcShelveset => GetReference("SourceTfvcShelveset");

    /// <summary>
    /// The local path on the agent where the test results are created.
    /// For example: c:\agent_work\1\TestResults
    ///
    /// This variable is agent-scoped, and can be used as an environment variable in a script and as a parameter in a build task, but not as part of the build number or as a version control tag.
    /// </summary>
    public VariableReference CommonTestResultsDirectory => GetReference("Common.TestResultsDirectory");

    /// <summary>
    /// Variables connected to why the build was created.
    /// </summary>
    public TriggeredByVariableReference TriggeredBy => new();
}

public sealed class RepositoryVariableReference : VariableReferenceBase
{
    protected override string Prefix => "Build.Repository.";

    internal RepositoryVariableReference()
    {
    }

    /// <summary>
    /// The value you've selected for Clean in the source repository settings.
    ///
    /// This variable is agent-scoped, and can be used as an environment variable in a script and as a parameter in a build task, but not as part of the build number or as a version control tag.
    /// </summary>
    public VariableReference Clean => GetReference("Clean");

    /// <summary>
    /// The local path on the agent where your source code files are downloaded.
    /// For example: c:\agent_work\1\s
    ///
    /// By default, new build pipelines update only the changed files.You can modify how files are downloaded on the Repository tab.
    /// Important note: If you check out only one Git repository, this path will be the exact path to the code.If you check out multiple repositories, the behavior is as follows(and might differ from the value of the Build.SourcesDirectory variable) :
    ///   - If the checkout step for the self(primary) repository has no custom checkout path defined, or the checkout path is the multi-checkout default path $(Pipeline.Workspace)/s/[RepoName] for the self repository, the value of this variable will revert to its default value, which is $(Pipeline.Workspace)/s.
    ///   - If the checkout step for the self (primary) repository does have a custom checkout path defined (and it's not its multi-checkout default path), this variable will contain the exact path to the self repository.
    ///
    /// This variable is agent-scoped, and can be used as an environment variable in a script and as a parameter in a build task, but not as part of the build number or as a version control tag.
    /// </summary>
    public VariableReference LocalPath => GetReference("LocalPath");

    /// <summary>
    /// The unique identifier of the repository.
    /// This won't change, even if the name of the repository does.
    ///
    /// This variable is agent-scoped, and can be used as an environment variable in a script and as a parameter in a build task, but not as part of the build number or as a version control tag.
    /// </summary>
    public VariableReference ID => GetReference("ID");

    /// <summary>
    /// The name of the triggering repository.
    ///
    /// This variable is agent-scoped, and can be used as an environment variable in a script and as a parameter in a build task, but not as part of the build number or as a version control tag.
    /// </summary>
    public VariableReference Name => GetReference("Name");

    /// <summary>
    /// The type of the triggering repository.
    ///   - TfsGit: TFS Git repository
    ///   - TfsVersionControl: Team Foundation Version Control
    ///   - Git: Git repository hosted on an external server
    ///   - GitHub
    ///   - Svn: Subversion
    ///
    /// This variable is agent-scoped, and can be used as an environment variable in a script and as a parameter in a build task, but not as part of the build number or as a version control tag.
    /// </summary>
    public VariableReference Provider => GetReference("Provider");

    /// <summary>
    /// Defined if your repository is Team Foundation Version Control. The name of the TFVC workspace used by the build agent.
    /// For example: if the Agent.BuildDirectory is c:\agent_work\12 and the Agent.Id is 8, the workspace name could be: ws_12_8
    ///
    /// This variable is agent-scoped, and can be used as an environment variable in a script and as a parameter in a build task, but not as part of the build number or as a version control tag.
    /// </summary>
    public VariableReference TfvcWorkspace => GetReference("Tfvc.Workspace");

    /// <summary>
    /// The URL for the triggering repository. For example:
    ///   - Git: https://dev.azure.com/fabrikamfiber/_git/Scripts
    ///   - TFVC: https://dev.azure.com/fabrikamfiber/
    ///
    /// This variable is agent-scoped, and can be used as an environment variable in a script and as a parameter in a build task, but not as part of the build number or as a version control tag.
    /// </summary>
    public VariableReference Uri => GetReference("Uri");

    /// <summary>
    /// The value you've selected for Checkout submodules on the repository tab. With multiple repos checked out, this value tracks the triggering repository's setting.
    ///
    /// This variable is agent-scoped, and can be used as an environment variable in a script and as a parameter in a build task, but not as part of the build number or as a version control tag.
    /// </summary>
    public VariableReference GitSubmoduleCheckout => GetReference("Git.SubmoduleCheckout");
}

public sealed class TriggeredByVariableReference : VariableReferenceBase
{
    protected override string Prefix => "Build.TriggeredBy.";

    internal TriggeredByVariableReference()
    {
    }

    /// <summary>
    /// If the build was triggered by another build, then this variable is set to the BuildID of the triggering build. In Classic pipelines, this variable is triggered by a build completion trigger.
    ///
    /// This variable is agent-scoped, and can be used as an environment variable in a script and as a parameter in a build task, but not as part of the build number or as a version control tag.
    /// If you are triggering a YAML pipeline using resources, you should use the resources variables instead.
    /// </summary>
    public VariableReference BuildId => GetReference("BuildId");

    /// <summary>
    /// If the build was triggered by another build, then this variable is set to the DefinitionID of the triggering build. In Classic pipelines, this variable is triggered by a build completion trigger.
    ///
    /// This variable is agent-scoped, and can be used as an environment variable in a script and as a parameter in a build task, but not as part of the build number or as a version control tag.
    /// If you are triggering a YAML pipeline using resources, you should use the resources variables instead.
    /// </summary>
    public VariableReference DefinitionId => GetReference("DefinitionId");

    /// <summary>
    /// If the build was triggered by another build, then this variable is set to the name of the triggering build pipeline. In Classic pipelines, this variable is triggered by a build completion trigger.
    ///
    /// This variable is agent-scoped, and can be used as an environment variable in a script and as a parameter in a build task, but not as part of the build number or as a version control tag.
    /// If you are triggering a YAML pipeline using resources, you should use the resources variables instead.
    /// </summary>
    public VariableReference DefinitionName => GetReference("DefinitionName");

    /// <summary>
    /// If the build was triggered by another build, then this variable is set to the number of the triggering build. In Classic pipelines, this variable is triggered by a build completion trigger.
    ///
    /// This variable is agent-scoped, and can be used as an environment variable in a script and as a parameter in a build task, but not as part of the build number or as a version control tag.
    /// If you are triggering a YAML pipeline using resources, you should use the resources variables instead.
    /// </summary>
    public VariableReference BuildNumber => GetReference("BuildNumber");

    /// <summary>
    /// If the build was triggered by another build, then this variable is set to ID of the project that contains the triggering build. In Classic pipelines, this variable is triggered by a build completion trigger.
    ///
    /// This variable is agent-scoped, and can be used as an environment variable in a script and as a parameter in a build task, but not as part of the build number or as a version control tag.
    /// If you are triggering a YAML pipeline using resources, you should use the resources variables instead.
    /// </summary>
    public VariableReference ProjectID => GetReference("ProjectID");
}

public sealed class SystemVariableReference : VariableReferenceBase
{
    protected override string Prefix => "System.";

    internal SystemVariableReference()
    {
    }

    /// <summary>
    /// Variables connected to a PR that initiated this build.
    /// </summary>
    public PullRequestVariableReference PullRequest { get; } = new();

    /// <summary>
    /// System.AccessToken is a special variable that carries the security token used by the running build.
    /// Use the OAuth token to access the REST API.
    /// Use System.AccessToken from YAML scripts.
    /// This variable is agent-scoped, and can be used as an environment variable in a script and as a parameter in a build task, but not as part of the build number or as a version control tag.
    /// </summary>
    public VariableReference AccessToken => GetReference("AccessToken");

    /// <summary>
    /// Variable that can be set for a pipeline run manually (either true or false).
    /// </summary>
    public VariableReference Debug => GetReference("Debug");

    /// <summary>
    /// The GUID of the TFS collection or Azure DevOps organization.
    /// </summary>
    public VariableReference CollectionId => GetReference("CollectionId");

    /// <summary>
    /// The URI of the TFS collection or Azure DevOps organization.
    /// For example: https://dev.azure.com/fabrikamfiber/.
    /// </summary>
    public VariableReference CollectionUri => GetReference("CollectionUri");

    /// <summary>
    /// The local path on the agent where your source code files are downloaded.
    /// For example: c:\agent_work\1\s
    /// By default, new build pipelines update only the changed files.
    /// You can modify how files are downloaded on the Repository tab.
    /// This variable is agent-scoped. It can be used as an environment variable in a script and as a parameter in a build task, but not as part of the build number or as a version control tag.
    /// </summary>
    public VariableReference DefaultWorkingDirectory => GetReference("DefaultWorkingDirectory");

    /// <summary>
    /// The ID of the build pipeline.
    /// </summary>
    public VariableReference DefinitionId => GetReference("DefinitionId");

    /// <summary>
    /// Set to build if the pipeline is a build. For a release, the values are deployment for a Deployment group job, gates during evaluation of gates, and release for other (Agent and Agentless) jobs.
    /// </summary>
    public VariableReference HostType => GetReference("HostType");

    /// <summary>
    /// Set to 1 the first time this job is attempted, and increments every time the job is retried.
    /// </summary>
    public VariableReference JobAttempt => GetReference("JobAttempt");

    /// <summary>
    /// The human-readable name given to a job.
    /// </summary>
    public VariableReference JobDisplayName => GetReference("JobDisplayName");

    /// <summary>
    /// A unique identifier for a single attempt of a single job. The value is unique to the current pipeline.
    /// </summary>
    public VariableReference JobId => GetReference("JobId");

    /// <summary>
    /// The name of the job, typically used for expressing dependencies and accessing output variables.
    /// </summary>
    public VariableReference JobName => GetReference("JobName");

    /// <summary>
    /// Set to 1 the first time this phase is attempted, and increments every time the job is retried.
    /// Note: "Phase" is a mostly-redundant concept which represents the design-time for a job (whereas job was the runtime version of a phase).
    /// We've mostly removed the concept of "phase" from Azure Pipelines. Matrix and multi-config jobs are the only place where "phase" is still distinct from "job".
    /// One phase can instantiate multiple jobs which differ only in their inputs.
    /// </summary>
    public VariableReference PhaseAttempt => GetReference("PhaseAttempt");

    /// <summary>
    /// The human-readable name given to a phase.
    /// </summary>
    public VariableReference PhaseDisplayName => GetReference("PhaseDisplayName");

    /// <summary>
    /// A string-based identifier for a job, typically used for expressing dependencies and accessing output variables.
    /// </summary>
    public VariableReference PhaseName => GetReference("PhaseName");

    /// <summary>
    /// Set to 1 the first time this stage is attempted, and increments every time the job is retried.
    /// </summary>
    public VariableReference StageAttempt => GetReference("StageAttempt");

    /// <summary>
    /// The human-readable name given to a stage.
    /// </summary>
    public VariableReference StageDisplayName => GetReference("StageDisplayName");

    /// <summary>
    /// A string-based identifier for a stage, typically used for expressing dependencies and accessing output variables.
    /// </summary>
    public VariableReference StageName => GetReference("StageName");

    /// <summary>
    /// The URI of the TFS collection or Azure DevOps organization.
    /// For example: https://dev.azure.com/fabrikamfiber/.
    /// This variable is agent-scoped, and can be used as an environment variable in a script and as a parameter in a build task, but not as part of the build number or as a version control tag.
    /// </summary>
    public VariableReference TeamFoundationCollectionUri => GetReference("TeamFoundationCollectionUri");

    /// <summary>
    /// The name of the project that contains this build.
    /// </summary>
    public VariableReference TeamProject => GetReference("TeamProject");

    /// <summary>
    /// The ID of the project that this build belongs to.
    /// </summary>
    public VariableReference TeamProjectId => GetReference("TeamProjectId");
}

public sealed class PullRequestVariableReference : VariableReferenceBase
{
    protected override string Prefix => "System.PullRequest.";

    internal PullRequestVariableReference()
    {
    }

    /// <summary>
    /// If the pull request is from a fork of the repository, this variable is set to True. Otherwise, it is set to False.
    /// </summary>
    public VariableReference IsFork => GetReference("IsFork");

    /// <summary>
    /// The ID of the pull request that caused this build.
    /// For example: 17.
    /// (This variable is initialized only if the build ran because of a Git PR affected by a branch policy).
    /// </summary>
    public VariableReference PullRequestId => GetReference("PullRequestId");

    /// <summary>
    /// The number of the pull request that caused this build.
    /// This variable is populated for pull requests from GitHub which have a different pull request ID and pull request number.
    /// This variable is only available in a YAML pipeline if the PR is a affected by a branch policy.
    /// </summary>
    public VariableReference PullRequestNumber => GetReference("PullRequestNumber");

    /// <summary>
    /// The branch that is being reviewed in a pull request.
    /// For example: refs/heads/users/raisa/new-feature for Azure Repos.
    /// (This variable is initialized only if the build ran because of a Git PR affected by a branch policy).
    /// This variable is only available in a YAML pipeline if the PR is affected by a branch policy.
    /// </summary>
    public VariableReference SourceBranch => GetReference("SourceBranch");

    /// <summary>
    /// The URL to the repo that contains the pull request.
    /// For example: https://dev.azure.com/ouraccount/_git/OurProject.
    /// </summary>
    public VariableReference SourceRepositoryURI => GetReference("SourceRepositoryURI");

    /// <summary>
    /// The branch that is the target of a pull request.
    /// For example: refs/heads/master when your repository is in Azure Repos and master when your repository is in GitHub.
    /// This variable is initialized only if the build ran because of a Git PR affected by a branch policy.
    /// This variable is only available in a YAML pipeline if the PR is affected by a branch policy.
    /// </summary>
    public VariableReference TargetBranch => GetReference("TargetBranch");
}

public sealed class PipelineVariableReference : VariableReferenceBase
{
    protected override string Prefix => "Pipeline.";

    internal PipelineVariableReference()
    {
    }

    /// <summary>
    /// Workspace directory for a particular pipeline. This variable has the same value as Agent.BuildDirectory.
    /// For example: /home/vsts/work/1
    /// </summary>
    public VariableReference Workspace => GetReference("Workspace");
}

public sealed class EnvironmentVariableReference : VariableReferenceBase
{
    protected override string Prefix => "Environment.";

    internal EnvironmentVariableReference()
    {
    }

    /// <summary>
    /// Name of the environment targeted in the deployment job to run the deployment steps and record the deployment history.
    /// For example: smarthotel-dev
    /// </summary>
    public VariableReference Name => GetReference("Name");

    /// <summary>
    /// ID of the environment targeted in the deployment job.
    /// For example: 10
    /// </summary>
    public VariableReference Id => GetReference("Id");

    /// <summary>
    /// Name of the specific resource within the environment targeted in the deployment job to run the deployment steps and record the deployment history.
    /// For example, bookings which is a Kubernetes namespace that has been added as a resource to the environment smarthotel-dev.
    /// </summary>
    public VariableReference ResourceName => GetReference("ResourceName");

    /// <summary>
    /// ID of the specific resource within the environment targeted in the deployment job to run the deployment steps.
    /// For example: 4
    /// </summary>
    public VariableReference ResourceId => GetReference("ResourceId");
}

public sealed class StrategyVariableReference : VariableReferenceBase
{
    protected override string Prefix => "Strategy.";

    internal StrategyVariableReference()
    {
    }

    /// <summary>
    /// The name of the deployment strategy: canary, runOnce, or rolling.
    /// </summary>
    public VariableReference Name => GetReference("Name");

    /// <summary>
    /// The current cycle name in a deployment: PreIteration, Iteration, or PostIteration.
    /// </summary>
    public VariableReference CycleName => GetReference("CycleName");
}

public sealed class ChecksVariableReference : VariableReferenceBase
{
    protected override string Prefix => "Checks.";

    internal ChecksVariableReference()
    {
    }

    /// <summary>
    /// Set to 1 the first time this stage is attempted, and increments every time the stage is retried.
    /// This variable can only be used within an approval or check for an environment.For example, you could use $(Checks.StageAttempt) within an Invoke REST API check.
    /// </summary>
    public VariableReference StageAttempt => GetReference("StageAttempt");
}
