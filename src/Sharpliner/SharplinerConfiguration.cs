using System.Collections.Generic;
using Sharpliner.Common;

namespace Sharpliner;

// This interface is needed to hide the Current static member.
public interface ISharplinerConfiguration
{
    /// <summary>
    /// Settings around YAML serialization
    /// </summary>
    SharplinerConfiguration.SerializationSettings Serialization { get; }

    /// <summary>
    /// Configuration of which validations run and with what severity
    /// </summary>
    SharplinerConfiguration.ValidationsSettings Validations { get; }

    /// <summary>
    /// Hook into the publishing process
    /// </summary>
    SharplinerConfiguration.SerializationHooks Hooks { get; }

    /// <summary>
    /// Registers a custom validation that is run for all definitions.
    /// </summary>
    void RegisterValidation<T>(T validation) where T : IDefinitionValidation;
}

/// <summary>
/// Inherit from this class to configure the publishing process.
/// You can customize how YAML is serialized, which validations are run and with what severity and more.
/// </summary>
public abstract class SharplinerConfiguration : ISharplinerConfiguration
{
    private readonly List<IDefinitionValidation> _additionalValidations = [];

    /// <summary>
    /// Settings around YAML serialization
    /// </summary>
    public class SerializationSettings
    {
        /// <summary>
        /// When true (default), inserts documentation headers into generated YAML files.
        /// The content of the headers can be customized via the Headers field on your definitions.
        /// </summary>
        public bool IncludeHeaders { get; set; } = true;

        /// <summary>
        /// When true (default), makes the YAML a little bit more human-readable.
        /// For instance, new lines are added.
        /// </summary>
        public bool PrettifyYaml { get; set; } = true;

        /// <summary>
        /// Set to false if you prefer Else branch to contain negated if condition rather than ${{ else }}
        /// </summary>
        public bool UseElseExpression { get; set; } = true;
    }

    /// <summary>
    /// Configuration of which validations run and with what severity
    /// </summary>
    public class ValidationsSettings
    {
        private readonly Dictionary<string, ValidationSeverity> _severities = new()
        {
            { nameof(NameFields), ValidationSeverity.Error },
            { nameof(DependsOnFields), ValidationSeverity.Warning },
            { nameof(RepositoryCheckouts), ValidationSeverity.Warning },
        };

        /// <summary>
        /// Validates whether stage and job names are valid.
        /// </summary>
        public ValidationSeverity NameFields
        {
            get => _severities[nameof(NameFields)];
            set => _severities[nameof(NameFields)] = value;
        }

        /// <summary>
        /// Validates whether stages and jobs do not dependent on each other and similar.
        /// </summary>
        public ValidationSeverity DependsOnFields
        {
            get => _severities[nameof(DependsOnFields)];
            set => _severities[nameof(DependsOnFields)] = value;
        }

        /// <summary>
        /// Validates whether checked out repositories are defined in resources.
        /// </summary>
        public ValidationSeverity RepositoryCheckouts
        {
            get => _severities[nameof(RepositoryCheckouts)];
            set => _severities[nameof(RepositoryCheckouts)] = value;
        }

        /// <summary>
        /// Sets validation severity for a specific validation.
        /// </summary>
        /// <param name="name">Validation name</param>
        internal ValidationSeverity this[string name]
        {
            get => _severities[name];
            set => _severities[name] = value;
        }
    }

    /// <summary>
    /// Hook into the publishing process
    /// </summary>
    public class SerializationHooks
    {
        public delegate void BeforePublishHandler(ISharplinerDefinition definition, string destinationPath);
        public delegate void AfterPublishHandler(ISharplinerDefinition definition, string destinationPath, string yaml);

        /// <summary>
        /// This hook gets called right before the YAML is published.
        /// Parameters passed are:
        ///   - The definition being published
        ///   - Destination path for the YAML file
        /// </summary>
        public BeforePublishHandler? BeforePublish { get; set; }

        /// <summary>
        /// This hook gets called right after the YAML is published.
        /// Parameters passed are:
        ///   - The definition being published
        ///   - Destination path for the YAML file
        ///   - The serialized YAML
        /// </summary>
        public AfterPublishHandler? AfterPublish { get; set; }
    }

    /// <summary>
    /// Current configuration we can reach from anywhere
    /// </summary>
    internal static SharplinerConfiguration Current { get; private set; } = new DefaultSharplinerConfiguration();

    /// <summary>
    /// Use this property to customize how YAML is serialized
    /// </summary>
    public SerializationSettings Serialization { get; } = new();

    /// <summary>
    /// Use this property to control which validations are run and with what severity
    /// </summary>
    public ValidationsSettings Validations { get; } = new();

    /// <summary>
    /// Use this property to hook into the publishing process
    /// </summary>
    public SerializationHooks Hooks { get; } = new();

    internal void ConfigureInternal()
    {
        Configure();
        Current = this;
    }

    public abstract void Configure();

    public void RegisterValidation<T>(T validation)
        where T : IDefinitionValidation
    {
        _additionalValidations.Add(validation);
    }

    internal IReadOnlyList<IDefinitionValidation> RegisteredValidations
        => _additionalValidations;
}

internal class DefaultSharplinerConfiguration : SharplinerConfiguration
{
    public override void Configure()
    {
    }
}
