using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Sharpliner.AzureDevOps.ConditionedExpressions;
using Sharpliner.AzureDevOps.ConditionedExpressions.Arguments;
using Sharpliner.AzureDevOps.Tasks;
using static Sharpliner.AzureDevOps.TemplateDefinition;

namespace Sharpliner.AzureDevOps;

/// <summary>
/// This is a common ancestor for AzDO related definitions (pipelines, templates..) containing useful macros.
/// </summary>
public abstract class AzureDevOpsDefinition
{
    #region Template references

    /// <summary>
    /// Reference a YAML template.
    /// </summary>
    /// <param name="path">Relative path to the YAML file with the template</param>
    /// <param name="parameters">Values for template parameters</param>
    protected static Template<VariableBase> VariableTemplate(string path, TemplateParameters? parameters = null)
        => new(path, parameters);

    /// <summary>
    /// Reference a YAML template.
    /// </summary>
    /// <param name="path">Relative path to the YAML file with the template</param>
    /// <param name="parameters">Values for template parameters</param>
    protected static Template<Stage> StageTemplate(string path, TemplateParameters? parameters = null)
        => new(path, parameters);

    /// <summary>
    /// Reference a YAML template.
    /// </summary>
    /// <param name="path">Relative path to the YAML file with the template</param>
    /// <param name="parameters">Values for template parameters</param>
    protected static Template<JobBase> JobTemplate(string path, TemplateParameters? parameters = null)
        => new(path, parameters);

    /// <summary>
    /// Reference a YAML template.
    /// </summary>
    /// <param name="path">Relative path to the YAML file with the template</param>
    /// <param name="parameters">Values for template parameters</param>
    protected static Template<Step> StepTemplate(string path, TemplateParameters? parameters = null)
        => new(path, parameters);

    #endregion

    #region Library references

    /// <summary>
    /// Reference a step library (series of library stages).
    /// </summary>
    protected static Conditioned<Stage> StageLibrary(StageLibrary library)
        => new LibraryReference<Stage>(library);

    /// <summary>
    /// Reference a step library (series of library jobs).
    /// </summary>
    protected static Conditioned<JobBase> JobLibrary(JobLibrary library)
        => new LibraryReference<JobBase>(library);

    /// <summary>
    /// Reference a step library (series of library steps).
    /// </summary>
    protected static Conditioned<Step> StepLibrary(StepLibrary library)
        => new LibraryReference<Step>(library);

    /// <summary>
    /// Reference a step library (series of library variables).
    /// </summary>
    protected static Conditioned<VariableBase> VariableLibrary(VariableLibrary library)
        => new LibraryReference<VariableBase>(library);

    /// <summary>
    /// Reference a step library (series of library stages).
    /// </summary>
    protected static Conditioned<Stage> ExpandStages(params Conditioned<Stage>[] stages)
        => new LibraryReference<Stage>(stages);

    /// <summary>
    /// Reference a step library (series of library jobs).
    /// </summary>
    protected static Conditioned<Job> ExpandJobs(params Conditioned<Job>[] jobs)
        => new LibraryReference<Job>(jobs);

    /// <summary>
    /// Reference a step library (series of library steps).
    /// </summary>
    protected static Conditioned<Step> ExpandSteps(params Conditioned<Step>[] steps)
        => new LibraryReference<Step>(steps);

    /// <summary>
    /// Reference a step library (series of library variables).
    /// </summary>
    protected static Conditioned<VariableBase> ExpandVariables(params Conditioned<VariableBase>[] variables)
        => new LibraryReference<VariableBase>(variables);

    /// <summary>
    /// Reference a step library (series of library stages).
    /// </summary>
    protected static Conditioned<Stage> ExpandStages(IEnumerable<Conditioned<Stage>> stages)
        => new LibraryReference<Stage>(stages);

    /// <summary>
    /// Reference a step library (series of library jobs).
    /// </summary>
    protected static Conditioned<Job> ExpandJobs(IEnumerable<Conditioned<Job>> jobs)
        => new LibraryReference<Job>(jobs);

    /// <summary>
    /// Reference a step library (series of library steps).
    /// </summary>
    protected static Conditioned<Step> ExpandSteps(IEnumerable<Conditioned<Step>> steps)
        => new LibraryReference<Step>(steps);

    /// <summary>
    /// Reference a step library (series of library variables).
    /// </summary>
    protected static Conditioned<VariableBase> ExpandVariables(IEnumerable<Conditioned<VariableBase>> variables)
        => new LibraryReference<VariableBase>(variables);

    /// <summary>
    /// Reference a step library (series of library stages).
    /// </summary>
    protected static Conditioned<Stage> ExpandStages(params Stage[] stages)
        => ExpandStages(stages.Select(x => new Conditioned<Stage>(x)));

    /// <summary>
    /// Reference a step library (series of library jobs).
    /// </summary>
    protected static Conditioned<Job> ExpandJobs(params Job[] jobs)
        => ExpandJobs(jobs.Select(x => new Conditioned<Job>(x)));

    /// <summary>
    /// Reference a step library (series of library steps).
    /// </summary>
    protected static Conditioned<Step> ExpandSteps(params Step[] steps)
        => ExpandSteps(steps.Select(x => new Conditioned<Step>(x)));

    /// <summary>
    /// Reference a step library (series of library variables).
    /// </summary>
    protected static Conditioned<VariableBase> ExpandVariables(params VariableBase[] variables)
        => ExpandVariables(variables.Select(x => new Conditioned<VariableBase>(x)));

    /// <summary>
    /// Reference a step library (series of library stages).
    /// </summary>
    protected static Conditioned<Stage> ExpandStages(IEnumerable<Stage> stages)
        => ExpandStages(stages.ToArray());

    /// <summary>
    /// Reference a step library (series of library jobs).
    /// </summary>
    protected static Conditioned<Job> ExpandJobs(IEnumerable<Job> jobs)
        => ExpandJobs(jobs.ToArray());

    /// <summary>
    /// Reference a step library (series of library steps).
    /// </summary>
    protected static Conditioned<Step> ExpandSteps(IEnumerable<Step> steps)
        => ExpandSteps(steps.ToArray());

    /// <summary>
    /// Reference a step library (series of library variables).
    /// </summary>
    protected static Conditioned<VariableBase> ExpandVariables(IEnumerable<VariableBase> variables)
        => ExpandVariables(variables.ToArray());

    /// <summary>
    /// Reference a stage library (series of library stages).
    /// </summary>
    protected static Conditioned<Stage> StageLibrary<T>() where T : StageLibrary, new()
        => CreateLibraryRef<T, Stage>();

    /// <summary>
    /// Reference a job library (series of library jobs).
    /// </summary>
    protected static Conditioned<JobBase> JobLibrary<T>() where T : JobLibrary, new()
        => CreateLibraryRef<T, JobBase>();

    /// <summary>
    /// Reference a step library (series of library steps).
    /// </summary>
    protected static Conditioned<Step> StepLibrary<T>() where T : StepLibrary, new()
        => CreateLibraryRef<T, Step>();

    /// <summary>
    /// Reference a variable library (set of variable definition).
    /// </summary>
    protected static Conditioned<VariableBase> VariableLibrary<T>() where T : VariableLibrary, new()
        => CreateLibraryRef<T, VariableBase>();

    /// <summary>
    /// Helper method to create instances of LibraryReference.
    /// </summary>
    /// <typeparam name="TLibrary">User's implementation type of the library</typeparam>
    /// <typeparam name="TDefinition">Definition type (Stage/Job/Step/Variable)</typeparam>
    internal static LibraryReference<TDefinition> CreateLibraryRef<TLibrary, TDefinition>()
        where TLibrary : DefinitionLibrary<TDefinition>, new()
        => new(CreateInstance<TLibrary>());

    #endregion

    #region Pipeline variable shorthands

    /// <summary>
    /// Allows the variables[""] notation for conditional definitions.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Should not be capitalized to follow YAML syntax")]
    protected static readonly VariablesReference variables = new();

    /// <summary>
    /// Defines a variable.
    /// </summary>
    /// <param name="name">Variable name</param>
    /// <param name="value">Variable value</param>
    protected static Conditioned<VariableBase> Variable(string name, string value) => new(new Variable(name, value));

    /// <summary>
    /// Defines a variable.
    /// </summary>
    /// <param name="name">Variable name</param>
    /// <param name="value">Variable value</param>
    protected static Conditioned<VariableBase> Variable(string name, int value) => new(new Variable(name, value));

    /// <summary>
    /// Defines a variable.
    /// </summary>
    /// <param name="name">Variable name</param>
    /// <param name="value">Variable value</param>
    protected static Conditioned<VariableBase> Variable(string name, bool value) => new(new Variable(name, value));

    /// <summary>
    /// References a variable group.
    /// </summary>
    /// <param name="name">Group name</param>
    protected static Conditioned<VariableBase> Group(string name) => new(new VariableGroup(name));

    #endregion

    #region Pipeline task shorthands

    /// <summary>
    /// Creates a bash task.
    /// </summary>
    protected static BashTaskBuilder Bash { get; } = new();

    /// <summary>
    /// Creates a script task.
    /// </summary>
    protected static ScriptTaskBuilder Script { get; } = new();

    /// <summary>
    /// Creates a powershell task.
    /// </summary>
    protected static PowershellTaskBuilder Powershell { get; } = new(false);

    /// <summary>
    /// Creates a pwsh task.
    /// </summary>
    protected static PowershellTaskBuilder Pwsh { get; } = new(true);

    /// <summary>
    /// Creates a publish task.
    /// </summary>
    protected static PublishTask Publish(string artifactName, string filePath, string? displayName = null)
        => new PublishTask(filePath) with
        {
            DisplayName = displayName!,
            Artifact = artifactName!,
        };

    /// <summary>
    /// Creates a checkout task.
    /// </summary>
    protected static CheckoutTaskBuilder Checkout { get; } = new();

    /// <summary>
    /// Creates a download task.
    /// </summary>
    protected static DownloadTaskBuilder Download { get; } = new();

    /// <summary>
    /// Creates a generic pipeline task.
    /// </summary>
    protected static AzureDevOpsTask Task(string taskName, string? displayName = null) => new AzureDevOpsTask(taskName) with { DisplayName = displayName! };

    /// <summary>
    /// Creates an UseDotNet or DotNetCoreCLI task.
    /// </summary>
    protected static DotNetTaskBuilder DotNet { get; } = new();

    /// <summary>
    /// This task verifies that you didn't forget to check in your YAML pipeline changes.
    /// </summary>
    /// <param name="pipelineProject">Path to the .csproj where pipelines are defined</param>
    protected static Step ValidateYamlsArePublished(string pipelineProject)
        => DotNet.Build(pipelineProject) with
        {
            DisplayName = "Validate YAML has been published",
            Arguments = $"-p:{nameof(PublishDefinitions.FailIfChanged)}=true"
        };

    #endregion

    #region Pipeline member shorthands

    /// <summary>
    /// Creates a new stage.
    /// </summary>
    protected static Stage Stage(string stageName, string? displayName = null) => new(stageName, displayName);

    /// <summary>
    /// Creates a new job.
    /// </summary>
    protected static Job Job(string jobName, string? displayName = null) => new(jobName, displayName);

    /// <summary>
    /// Creates a new deployment job.
    /// </summary>
    protected static DeploymentJob DeploymentJob(string jobName, string? displayName = null) => new(jobName, displayName);

    #endregion

    #region Pipeline parameter shorthands

    /// <summary>
    /// Allows the ${{ parameters.name }} notation for parameter reference.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Should not be capitalized to follow YAML syntax")]
    protected static readonly TemplateParameterReference parameters = new();

    /// <summary>
    /// Defines a string template parameter
    /// </summary>
    /// <param name="name">Name of the parameter, can be referenced in the template as ${{ parameters.name }}</param>
    /// <param name="displayName">Display name of the parameter shown in the UI when creating pipeline run</param>
    /// <param name="defaultValue">Default value; if no default, then the parameter MUST be given by the user at runtime</param>
    /// <param name="allowedValues">Allowed list of values (for some data types)</param>
    protected static Parameter StringParameter(string name, string? displayName = null, string? defaultValue = null, IEnumerable<string>? allowedValues = null)
        => new StringParameter(name, displayName, defaultValue, allowedValues);

    /// <summary>
    /// Defines a number template parameter
    /// </summary>
    /// <param name="name">Name of the parameter, can be referenced in the template as ${{ parameters.name }}</param>
    /// <param name="displayName">Display name of the parameter shown in the UI when creating pipeline run</param>
    /// <param name="defaultValue">Default value; if no default, then the parameter MUST be given by the user at runtime</param>
    /// <param name="allowedValues">Allowed list of values (for some data types)</param>
    protected static Parameter NumberParameter(string name, string? displayName = null, int defaultValue = 0, IEnumerable<int?>? allowedValues = null)
        => new NumberParameter(name, displayName, defaultValue, allowedValues);

    /// <summary>
    /// Defines a boolean template parameter
    /// </summary>
    /// <param name="name">Name of the parameter, can be referenced in the template as ${{ parameters.name }}</param>
    /// <param name="displayName">Display name of the parameter shown in the UI when creating pipeline run</param>
    /// <param name="defaultValue">Default value; if no default, then the parameter MUST be given by the user at runtime</param>
    protected static Parameter BooleanParameter(string name, string? displayName = null, bool? defaultValue = null)
        => new BooleanParameter(name, displayName, defaultValue);

    /// <summary>
    /// Defines a object template parameter
    /// </summary>
    /// <param name="name">Name of the parameter, can be referenced in the template as ${{ parameters.name }}</param>
    /// <param name="displayName">Display name of the parameter shown in the UI when creating pipeline run</param>
    /// <param name="defaultValue">Default value; if no default, then the parameter MUST be given by the user at runtime</param>
    protected static Parameter ObjectParameter(string name, string? displayName = null, ConditionedDictionary? defaultValue = null)
        => new ObjectParameter(name, displayName, defaultValue);

    /// <summary>
    /// Defines a step template parameter
    /// </summary>
    /// <param name="name">Name of the parameter, can be referenced in the template as ${{ parameters.name }}</param>
    /// <param name="displayName">Display name of the parameter shown in the UI when creating pipeline run</param>
    /// <param name="defaultValue">Default value; if no default, then the parameter MUST be given by the user at runtime</param>
    protected static Parameter StepParameter(string name, string? displayName = null, Step? defaultValue = null)
        => new StepParameter(name, displayName, defaultValue);

    /// <summary>
    /// Defines a stepList template parameter
    /// </summary>
    /// <param name="name">Name of the parameter, can be referenced in the template as ${{ parameters.name }}</param>
    /// <param name="displayName">Display name of the parameter shown in the UI when creating pipeline run</param>
    /// <param name="defaultValue">Default value; if no default, then the parameter MUST be given by the user at runtime</param>
    protected static Parameter StepListParameter(string name, string? displayName = null, ConditionedList<Step>? defaultValue = null)
        => new StepListParameter(name, displayName, defaultValue);

    /// <summary>
    /// Defines a job template parameter
    /// </summary>
    /// <param name="name">Name of the parameter, can be referenced in the template as ${{ parameters.name }}</param>
    /// <param name="displayName">Display name of the parameter shown in the UI when creating pipeline run</param>
    /// <param name="defaultValue">Default value; if no default, then the parameter MUST be given by the user at runtime</param>
    protected static Parameter JobParameter(string name, string? displayName = null, JobBase? defaultValue = null)
        => new JobParameter(name, displayName, defaultValue);

    /// <summary>
    /// Defines a jobList template parameter
    /// </summary>
    /// <param name="name">Name of the parameter, can be referenced in the template as ${{ parameters.name }}</param>
    /// <param name="displayName">Display name of the parameter shown in the UI when creating pipeline run</param>
    /// <param name="defaultValue">Default value; if no default, then the parameter MUST be given by the user at runtime</param>
    protected static Parameter JobListParameter(string name, string? displayName = null, ConditionedList<JobBase>? defaultValue = null)
        => new JobListParameter(name, displayName, defaultValue);

    /// <summary>
    /// Defines a deployment job template parameter
    /// </summary>
    /// <param name="name">Name of the parameter, can be referenced in the template as ${{ parameters.name }}</param>
    /// <param name="displayName">Display name of the parameter shown in the UI when creating pipeline run</param>
    /// <param name="defaultValue">Default value; if no default, then the parameter MUST be given by the user at runtime</param>
    protected static Parameter DeploymentParameter(string name, string? displayName = null, DeploymentJob? defaultValue = null)
        => new DeploymentParameter(name, displayName, defaultValue);

    /// <summary>
    /// Defines a deploymentList template parameter
    /// </summary>
    /// <param name="name">Name of the parameter, can be referenced in the template as ${{ parameters.name }}</param>
    /// <param name="displayName">Display name of the parameter shown in the UI when creating pipeline run</param>
    /// <param name="defaultValue">Default value; if no default, then the parameter MUST be given by the user at runtime</param>
    protected static Parameter DeploymentListParameter(string name, string? displayName = null, ConditionedList<DeploymentJob>? defaultValue = null)
        => new DeploymentListParameter(name, displayName, defaultValue);

    /// <summary>
    /// Defines a stage template parameter
    /// </summary>
    /// <param name="name">Name of the parameter, can be referenced in the template as ${{ parameters.name }}</param>
    /// <param name="displayName">Display name of the parameter shown in the UI when creating pipeline run</param>
    /// <param name="defaultValue">Default value; if no default, then the parameter MUST be given by the user at runtime</param>
    protected static Parameter StageParameter(string name, string? displayName = null, Stage? defaultValue = null)
        => new StageParameter(name, displayName, defaultValue);

    /// <summary>
    /// Defines a stageList template parameter
    /// </summary>
    /// <param name="name">Name of the parameter, can be referenced in the template as ${{ parameters.name }}</param>
    /// <param name="displayName">Display name of the parameter shown in the UI when creating pipeline run</param>
    /// <param name="defaultValue">Default value; if no default, then the parameter MUST be given by the user at runtime</param>
    protected static Parameter StageListParameter(string name, string? displayName = null, ConditionedList<Stage>? defaultValue = null)
        => new StageListParameter(name, displayName, defaultValue);

    #endregion

    #region Conditions

    /// <summary>
    /// Start an ${{ if () }} section.
    /// </summary>
    protected static IfConditionBuilder If => new();

    /// <summary>
    /// Use this to specify any custom condition (in case you miss some operator or expression).
    /// </summary>
    protected static InlineCondition Condition(string condition) => new InlineCustomCondition(condition);

    protected static InlineCondition<T> And<T>(params string[] expressions) => new InlineAndCondition<T>(expressions);

    protected static InlineCondition Or<T>(params string[] expressions) => new InlineOrCondition<T>(expressions);

    protected static InlineCondition Xor<T>(string expression1, string expression2) => new InlineXorCondition<T>(expression1, expression2);

    protected static InlineCondition And<T>(params InlineCondition[] expressions) => new InlineAndCondition<T>(expressions);

    protected static InlineCondition Or<T>(params InlineCondition[] expressions) => new InlineOrCondition<T>(expressions);

    protected static InlineCondition Xor<T>(Condition expression1, Condition expression2) => new InlineXorCondition<T>(expression1, expression2);

    protected static InlineCondition Equal<T>(InlineExpression expression1, InlineExpression expression2) => new InlineEqualityCondition<T>(true, expression1, expression2);

    protected static InlineCondition NotEqual<T>(InlineExpression expression1, InlineExpression expression2) => new InlineEqualityCondition<T>(false, expression1, expression2);

    protected static InlineCondition Contains<T>(InlineExpression needle, InlineExpression haystack) => new InlineContainsCondition<T>(needle, haystack);

    protected static InlineCondition StartsWith<T>(InlineExpression needle, InlineExpression haystack) => new InlineStartsWithCondition<T>(needle, haystack);

    protected static InlineCondition EndsWith<T>(InlineExpression needle, InlineExpression haystack) => new InlineEndsWithCondition<T>(needle, haystack);

    protected static InlineCondition ContainsValue<T>(InlineExpression needle, params InlineExpression[] haystack) => new InlineContainsValueCondition<T>(needle, haystack);

    protected static InlineCondition In<T>(InlineExpression needle, params InlineExpression[] haystack) => new InlineInCondition<T>(needle, haystack);

    protected static InlineCondition NotIn<T>(InlineExpression needle, params InlineExpression[] haystack) => new InlineNotInCondition<T>(needle, haystack);

    protected static InlineCondition Greater<T>(InlineExpression expression1, InlineExpression expression2) => new InlineGreaterCondition<T>(expression1, expression2);

    protected static InlineCondition Less<T>(InlineExpression expression1, InlineExpression expression2) => new InlineLessCondition<T>(expression1, expression2);

    protected static InlineCondition And(params string[] expressions) => new InlineAndCondition(expressions);

    protected static InlineCondition Or(params string[] expressions) => new InlineOrCondition(expressions);

    protected static InlineCondition Xor(string condition1, string condition2) => new InlineXorCondition(condition1, condition2);

    protected static InlineCondition And(params InlineCondition[] expressions) => new InlineAndCondition(expressions);

    protected static InlineCondition Or(params InlineCondition[] expressions) => new InlineOrCondition(expressions);

    protected static InlineCondition Xor(InlineCondition expression1, InlineCondition expression2) => new InlineXorCondition(expression1, expression2);

    protected static InlineCondition Contains(InlineExpression needle, InlineExpression haystack) => new InlineContainsCondition(needle, haystack);

    protected static InlineCondition StartsWith(InlineExpression needle, InlineExpression haystack) => new InlineStartsWithCondition(needle, haystack);

    protected static InlineCondition EndsWith(InlineExpression needle, InlineExpression haystack) => new InlineEndsWithCondition(needle, haystack);

    protected static InlineCondition In(InlineExpression needle, params InlineExpression[] haystack) => new InlineInCondition(needle, haystack);

    protected static InlineCondition NotIn(InlineExpression needle, params InlineExpression[] haystack) => new InlineNotInCondition(needle, haystack);

    protected static InlineCondition ContainsValue(InlineExpression needle, params InlineExpression[] haystack) => new InlineContainsValueCondition(needle, haystack);

    protected static InlineCondition Equal(InlineExpression expression1, InlineExpression expression2) => new InlineEqualityCondition(true, expression1, expression2);

    protected static InlineCondition NotEqual(InlineExpression expression1, InlineExpression expression2) => new InlineEqualityCondition(false, expression1, expression2);

    protected static InlineCondition Greater(InlineExpression expression1, InlineExpression expression2) => new InlineGreaterCondition(expression1, expression2);

    protected static InlineCondition Less(InlineExpression expression1, InlineExpression expression2) => new InlineLessCondition(expression1, expression2);

    protected static InlineCondition IsBranch(InlineExpression branchName) => new InlineBranchCondition(branchName, true);

    protected static InlineCondition IsNotBranch(InlineExpression branchName) => new InlineBranchCondition(branchName, false);

    protected static InlineCondition IsPullRequest => new InlineBuildReasonCondition("PullRequest", true);

    protected static InlineCondition IsNotPullRequest => new InlineBuildReasonCondition("PullRequest", false);

    #endregion

    #region Helpers

    internal static readonly Regex NameRegex = new("^[A-Za-z0-9_]+$", RegexOptions.Compiled);

    /// <summary>
    /// AzDO allows an empty dependsOn which then forces the stage/job to kick off in parallel.
    /// If dependsOn is omitted, stages/jobs run in the order they are defined.
    /// </summary>
    protected static ConditionedList<string> NoDependsOn => new EmptyDependsOn();

    /// <summary>
    /// Helper method to create instances of T.
    /// </summary>
    internal static T CreateInstance<T>() where T : new() => (T)Activator.CreateInstance(typeof(T))!;

    #endregion
}
