using System.Collections.Generic;
using FluentAssertions;
using Sharpliner.AzureDevOps;
using Sharpliner.AzureDevOps.ConditionedExpressions;
using Xunit;

namespace Sharpliner.Tests.AzureDevOps;

public class LibraryTests
{
    private class Job_Library : JobLibrary
    {
        public override List<Conditioned<JobBase>> Jobs => new()
        {
            new Job("Start")
            {
                Steps =
                {
                    Script.Inline("echo 'Hello World'")
                }
            },

            new Job("End")
            {
                Steps = new[]
                {
                    Script.Inline("echo 'Goodbye World'")
                }
            }
        };
    }

    private class JobReferencingPipeline : SimpleTestPipeline
    {
        public override SingleStagePipeline Pipeline => new()
        {
            Jobs =
            {
                new Job("Init"),

                JobLibrary<Job_Library>()
            }
        };
    }

    [Fact]
    public void Job_Library_Test()
    {
        var yaml = new JobReferencingPipeline().Serialize();

        yaml.Trim().Should().Be(
            """
            jobs:
            - job: Init

            - job: Start
              steps:
              - script: |-
                  echo 'Hello World'

            - job: End
              steps:
              - script: |-
                  echo 'Goodbye World'
            """);
    }

    private class DotNet_Step_Library : StepLibrary
    {
        public override List<Conditioned<Step>> Steps => new()
        {
            DotNet.Install.Sdk("6.0.100"),

            If.IsBranch("main")
                .Step(DotNet.Restore.Projects("src/MyProject.sln")),

            DotNet.Build("src/MyProject.sln"),
        };
    }

    private class SimpleDotNetPipeline : SimpleTestPipeline
    {
        public override SingleStagePipeline Pipeline => new()
        {
            Jobs =
            {
                new Job("Foo")
                {
                    Steps =
                    {
                        Script.Inline("echo 'Hello World'"),

                        StepLibrary<DotNet_Step_Library>(),

                        Script.Inline("echo 'Goodbye World'"),
                    }
                }
            }
        };
    }

    [Fact]
    public void Step_Library_Test()
    {
        var yaml = new SimpleDotNetPipeline().Serialize();

        yaml.Trim().Should().Be(
            """
            jobs:
            - job: Foo
              steps:
              - script: |-
                  echo 'Hello World'

              - task: UseDotNet@2
                inputs:
                  packageType: sdk
                  version: 6.0.100

              - ${{ if eq(variables['Build.SourceBranch'], 'refs/heads/main') }}:
                - task: DotNetCoreCLI@2
                  inputs:
                    command: restore
                    projects: src/MyProject.sln

              - task: DotNetCoreCLI@2
                inputs:
                  command: build
                  projects: src/MyProject.sln

              - script: |-
                  echo 'Goodbye World'
            """);
    }

    private class Variable_Library : VariableLibrary
    {
        private readonly string _env;

        public Variable_Library(string env)
        {
            _env = env;
        }

        public override List<Conditioned<VariableBase>> Variables => new()
        {
            Variable($"connection-string-{_env}", $"{_env}_123"),

            If.IsBranch(_env)
                .Group("prod-kv")
        };
    }

    private class ConditionalPipeline : SimpleTestPipeline
    {
        public override SingleStagePipeline Pipeline => new()
        {
            Variables =
            {
                Variable("test", true),

                If.NotIn("'$(Environment)'", "'prod'")
                    .VariableLibrary(new Variable_Library("dev"))
                    .VariableLibrary(new Variable_Library("staging"))
                .Else
                    .VariableLibrary(new Variable_Library("prod"))
            }
        };
    }

    [Fact]
    public void Conditional_Library_Test()
    {
        var yaml = new ConditionalPipeline().Serialize();

        yaml.Trim().Should().Be(
            """
            variables:
            - name: test
              value: true

            - ${{ if notIn('$(Environment)', 'prod') }}:
              - name: connection-string-dev
                value: dev_123

              - ${{ if eq(variables['Build.SourceBranch'], 'refs/heads/dev') }}:
                - group: prod-kv

              - name: connection-string-staging
                value: staging_123

              - ${{ if eq(variables['Build.SourceBranch'], 'refs/heads/staging') }}:
                - group: prod-kv

            - ${{ else }}:
              - name: connection-string-prod
                value: prod_123

              - ${{ if eq(variables['Build.SourceBranch'], 'refs/heads/prod') }}:
                - group: prod-kv
            """);
    }
}
