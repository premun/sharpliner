using System.Linq;
using FluentAssertions;
using Sharpliner.AzureDevOps;
using Xunit;

namespace Sharpliner.Tests.AzureDevOps.ConditionedExpressions;

public class ConditionalsTests
{
    private class And_Condition_Test_Pipeline : TestPipeline
    {
        public override Pipeline Pipeline => new()
        {
            Variables =
            {
                If.And(
                    NotIn("bar", "foo", "xyz", "'foo'"),
                    NotEqual(variables.Configuration, "'Debug'"),
                    ContainsValue("10", variables.System.JobId)
                )
                    .Variable("TargetBranch", "$(System.PullRequest.SourceBranch)"),
            }
        };
    }

    [Fact]
    public void And_Condition_Test()
    {
        var pipeline = new And_Condition_Test_Pipeline();
        var variable = pipeline.Pipeline.Variables.First();
        variable.Condition!.WithoutTags().Should().Be(
            "and(" +
                "notIn('bar', 'foo', 'xyz', 'foo'), " +
                "ne(variables['Configuration'], 'Debug'), " +
                "containsValue(variables['System.JobId'], 10))");
    }

    private class Or_Condition_Test_Pipeline : TestPipeline
    {
        public override Pipeline Pipeline => new()
        {
            Variables =
            {
                If.Or(
                    And(
                        Less("5", "3"),
                        Equal(variables.Build.SourceBranch, "'refs/heads/production'"),
                        IsBranch("release")),
                    NotEqual(variables.Configuration, "'Debug'"),
                    IsPullRequest)
                    .Variable("TargetBranch", variables.System.PullRequest.SourceBranch),
            }
        };
    }

    [Fact]
    public void Or_Condition_Test()
    {
        var pipeline = new Or_Condition_Test_Pipeline();
        var variable = pipeline.Pipeline.Variables.First();
        variable.Condition!.WithoutTags().Should().Be(
            "or(" +
                "and(" +
                    "lt(5, 3), " +
                    "eq(variables['Build.SourceBranch'], 'refs/heads/production'), " +
                    "eq(variables['Build.SourceBranch'], 'refs/heads/release')), " +
                "ne(variables['Configuration'], 'Debug'), " +
                "eq(variables['Build.Reason'], 'PullRequest'))");
    }

    private class Branch_Condition_Test_Pipeline : TestPipeline
    {
        public override Pipeline Pipeline => new()
        {
            Variables =
            {
                If.IsBranch("main")
                    .Variable("feature", "on"),

                If.And(IsPullRequest, IsNotBranch("main"))
                    .Group("pr-group"),
            }
        };
    }

    [Fact]
    public void Branch_Condition_Test()
    {
        var pipeline = new Branch_Condition_Test_Pipeline();

        var variable1 = pipeline.Pipeline.Variables.ElementAt(0);
        var variable2 = pipeline.Pipeline.Variables.ElementAt(1);

        variable1.Condition!.WithoutTags().Should().Be("eq(variables['Build.SourceBranch'], 'refs/heads/main')");
        variable2.Condition!.WithoutTags().Should().Be("and(eq(variables['Build.Reason'], 'PullRequest'), ne(variables['Build.SourceBranch'], 'refs/heads/main'))");
    }

    private class Else_Test_Pipeline : TestPipeline
    {
        public override Pipeline Pipeline => new()
        {
            Variables =
            {
                If.Equal("a", "b")
                    .Variable("feature", "on")
                    .Variable("feature2", "on")
                .Else
                    .Variable("feature", "off")
                    .Variable("feature2", "off"),

                If.ContainsValue("'foo'", "'bar'", "'foo'", "'xyz'")
                    .Variable("feature", "on")
                    .Variable("feature2", "on")
                    .If.And(Equal("e", "f"), NotEqual("g", "h"))
                        .Variable("feature", "on")
                        .Variable("feature2", "on")
                    .Else
                        .Variable("feature", "off")
                        .Variable("feature2", "off"),
            }
        };
    }

    [Fact]
    public void Else_Test()
    {
        var pipeline = new Else_Test_Pipeline();
        pipeline.Serialize().Trim().Should().Be(
            """
            variables:
            - ${{ if eq('a', 'b') }}:
              - name: feature
                value: on

              - name: feature2
                value: on

            - ${{ else }}:
              - name: feature
                value: off

              - name: feature2
                value: off

            - ${{ if containsValue('bar', 'foo', 'xyz', 'foo') }}:
              - name: feature
                value: on

              - name: feature2
                value: on

              - ${{ if and(eq('e', 'f'), ne('g', 'h')) }}:
                - name: feature
                  value: on

                - name: feature2
                  value: on

              - ${{ else }}:
                - name: feature
                  value: off

                - name: feature2
                  value: off
            """);
    }

    private class ConditionedValueWithElse_Pipeline : SimpleTestPipeline
    {
        public override SingleStagePipeline Pipeline => new()
        {
            Jobs =
            {
                new Job("Job")
                {
                    Pool = If.Equal("A", "B")
                                .Pool(new HostedPool("pool-A")
                                {
                                    Demands = { "SomeProperty -equals SomeValue" }
                                })
                            .Else
                                .Pool(new HostedPool("pool-B")),
                }
            }
        };
    }

    [Fact]
    public void ConditionedValueWithElse_Test()
    {
        var pipeline = new ConditionedValueWithElse_Pipeline();
        pipeline.Serialize().Trim().Should().Be(
            """
            jobs:
            - job: Job
              pool:
                ${{ if eq('A', 'B') }}:
                  name: pool-A
                  demands:
                  - SomeProperty -equals SomeValue
                ${{ else }}:
                  name: pool-B
            """);
    }

    private class ConditionedValueWithElseIf_Pipeline : SimpleTestPipeline
    {
        public override SingleStagePipeline Pipeline => new()
        {
            Jobs =
            {
                new Job("Job")
                {
                    Pool = If.Equal("A", "B")
                                .Pool(new HostedPool("pool-A")
                                {
                                    Demands = { "SomeProperty -equals SomeValue" }
                                })
                            .EndIf
                            .If.Equal("C", "D")
                                .Pool(new HostedPool("pool-B")),
                }
            }
        };
    }

    [Fact]
    public void ConditionedValueWithElseIf_Test()
    {
        var pipeline = new ConditionedValueWithElseIf_Pipeline();
        pipeline.Serialize().Trim().Should().Be(
            """
            jobs:
            - job: Job
              pool:
                ${{ if eq('A', 'B') }}:
                  name: pool-A
                  demands:
                  - SomeProperty -equals SomeValue
                ${{ if eq('C', 'D') }}:
                  name: pool-B
            """);
    }

    private class Custom_Condition_Test_Pipeline : TestPipeline
    {
        public override Pipeline Pipeline => new()
        {
            Variables =
            {
                If.Condition("containsValue($(System.User), 'azdobot')")
                    .Variable("TargetBranch", "$(System.PullRequest.SourceBranch)"),

                If.Condition(In("'foo'", "'bar'"))
                    .Variable("TargetBranch", "production"),

                If.Condition(Xor("True", "$(Variable)"))
                    .Variable("TargetBranch", "main"),
            }
        };
    }

    [Fact]
    public void Custom_Condition_Test()
    {
        var pipeline = new Custom_Condition_Test_Pipeline();
        var variable = pipeline.Pipeline.Variables.First();
        variable.Condition!.WithoutTags().Should().Be("containsValue($(System.User), 'azdobot')");
        variable = pipeline.Pipeline.Variables.ElementAt(1);
        variable.Condition!.WithoutTags().Should().Be("in('foo', 'bar')");
        variable = pipeline.Pipeline.Variables.Last();
        variable.Condition!.WithoutTags().Should().Be("xor(True, $(Variable))");
    }

    private class Contains_Condition_Test_Pipeline : TestPipeline
    {
        public override Pipeline Pipeline => new()
        {
            Variables =
            {
                If.Contains("refs/heads/feature/", variables.Build.SourceBranch)
                    .Variable("feature", "on"),

                If.Contains("refs/heads/feature/", variables["Build.SourceBranch"])
                    .Variable("feature", "on")
            }
        };
    }

    [Fact]
    public void Contains_Condition_Test()
    {
        var pipeline = new Contains_Condition_Test_Pipeline();

        var variable1 = pipeline.Pipeline.Variables.ElementAt(0);
        var variable2 = pipeline.Pipeline.Variables.ElementAt(1);

        variable1.Condition!.WithoutTags().Should().Be("contains(variables['Build.SourceBranch'], 'refs/heads/feature/')");
        variable2.Condition!.WithoutTags().Should().Be("contains(variables['Build.SourceBranch'], 'refs/heads/feature/')");
    }

    private class StartsWith_Condition_Test_Pipeline : TestPipeline
    {
        public override Pipeline Pipeline => new()
        {
            Variables =
            {
                If.StartsWith("refs/heads/feature/", variables.Build.SourceBranch)
                    .Variable("feature", "on"),

                If.StartsWith("refs/heads/feature/", variables["Build.SourceBranch"])
                    .Variable("feature", "on")
            }
        };
    }

    [Fact]
    public void StartsWith_Condition_Test()
    {
        var pipeline = new StartsWith_Condition_Test_Pipeline();

        var variable1 = pipeline.Pipeline.Variables.ElementAt(0);
        var variable2 = pipeline.Pipeline.Variables.ElementAt(1);

        variable1.Condition!.WithoutTags().Should().Be("startsWith(variables['Build.SourceBranch'], 'refs/heads/feature/')");
        variable2.Condition!.WithoutTags().Should().Be("startsWith(variables['Build.SourceBranch'], 'refs/heads/feature/')");
    }

    private class EndsWith_Condition_Test_Pipeline : TestPipeline
    {
        public override Pipeline Pipeline => new()
        {
            Variables =
            {
                If.EndsWith("refs/heads/feature/", variables.Build.SourceBranch)
                    .Variable("feature", "on"),

                If.EndsWith("refs/heads/feature/", variables["Build.SourceBranch"])
                    .Variable("feature", "on")
            }
        };
    }

    [Fact]
    public void EndsWith_Condition_Test()
    {
        var pipeline = new EndsWith_Condition_Test_Pipeline();

        var variable1 = pipeline.Pipeline.Variables.ElementAt(0);
        var variable2 = pipeline.Pipeline.Variables.ElementAt(1);

        variable1.Condition!.WithoutTags().Should().Be("endsWith(variables['Build.SourceBranch'], 'refs/heads/feature/')");
        variable2.Condition!.WithoutTags().Should().Be("endsWith(variables['Build.SourceBranch'], 'refs/heads/feature/')");
    }

    private class IfCondition_And_InlineCondition_Test_Pipeline : TestPipeline
    {
        public override Pipeline Pipeline => new()
        {
            Parameters = { StringParameter("Param1", defaultValue: "ParamValue1") },
            Variables =
            {
                If.StartsWith("Param", parameters["Param1"])
                    .Variable("feature", "on"),
            },
            Stages =
            {
                new Stage("Stage1")
                {
                    Jobs =
                    {
                        new Job("Job1")
                        {
                            Steps =
                            {
                                Script.Inline("echo Does this condition work?") with
                                {
                                    Condition = StartsWith("Param", parameters["Param1"])
                                }
                            },
                            Condition = StartsWith("Param", parameters["Param1"])
                        }
                    },
                    Condition = StartsWith("Param", parameters["Param1"])
                }
            }
        };
    }

    [Fact]
    public void IfCondition_And_InlineCondition_Test()
    {
        var pipeline = new IfCondition_And_InlineCondition_Test_Pipeline();

        var yaml = pipeline.Serialize();

        yaml.Trim().Should().Be(
            """
            parameters:
            - name: Param1
              type: string
              default: ParamValue1

            variables:
            - ${{ if startsWith(parameters.Param1, 'Param') }}:
              - name: feature
                value: on

            stages:
            - stage: Stage1
              jobs:
              - job: Job1
                steps:
                - script: |-
                    echo Does this condition work?
                  condition: startsWith('${{ parameters.Param1 }}', 'Param')
                condition: startsWith('${{ parameters.Param1 }}', 'Param')
              condition: startsWith('${{ parameters.Param1 }}', 'Param')
            """
            );
    }

}
