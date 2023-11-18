using MattEland.BatComputer.Abstractions.Strategies;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.Planning;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MattEland.BatComputer.Kernel;

public static class ExecutionResultExtensions
{
    public static PlanExecutionResult ToExecutionResult(this FunctionResult result, Plan plan)
    {
        PlanExecutionResult executionResult = new();

        return PopulateExecutionResult(executionResult, result, plan);
    }

    private static PlanExecutionResult PopulateExecutionResult(PlanExecutionResult executionResult, FunctionResult result, Plan plan)
    {
        if (result.TryGetMetadataValue("stepCount", out string stepCount))
        {
            executionResult.StepsCount = int.Parse(stepCount);
        }
        else if (plan.Steps != null && plan.Steps.Any())
        {
            executionResult.StepsCount = plan.Steps.Count;
        }

        if (result.TryGetMetadataValue("stepsTaken", out string json))
        {
            executionResult.Summary = DeserializeStepsTaken(json);
        }
        else if (plan.Steps != null && plan.Steps.Any())
        {
            executionResult.Summary = plan.Steps.Select(step => new StepSummary
                {
                    Action = step.Name,
                    Thought = step.Description,
                    Observation = GetOutputObservations(step, plan),
                    ActionVariables = GetInputVariables(step, plan),
                }).ToList();
        }

        if (result.TryGetMetadataValue("functionCount", out string functionCount))
        {
            executionResult.FunctionsUsed = functionCount;
        }
        else if (plan.Steps != null && plan.Steps.Any())
        {
            executionResult.FunctionsUsed = plan.Steps.Count.ToString();
        }

        if (result.TryGetMetadataValue("iterations", out string iterations))
        {
            executionResult.Iterations = int.Parse(iterations);
        }
        else
        {
            executionResult.Iterations = 1;
        }

        executionResult.Output = result.GetValue<string>();
        if (string.IsNullOrEmpty(executionResult.Output))
        {
            executionResult.Output = "I'm sorry, but I can't think of anything.";
        }

        return executionResult;
    }

    private static Dictionary<string, string?> GetInputVariables(Plan step, Plan plan)
    {
        Dictionary<string, string?> results = new();

        foreach (KeyValuePair<string, string> kvp in step.Parameters)
        {
            if (!string.IsNullOrEmpty(kvp.Value))
            {
                results[kvp.Key] = kvp.Value;
            } 
            else if (plan.State.TryGetValue(kvp.Key, out string? value))
            {
                results[kvp.Key] = value;
            }
            else
            {
                results[kvp.Key] = null;
            }
        }

        return results;
    }

    private static string GetOutputObservations(Plan step, Plan plan)
    {
        StringBuilder sb = new();

        foreach (string output in step.Outputs)
        {
            if (sb.Length > 0)
            {
                sb.Append(", ");
            }
            if (plan.State.TryGetValue(output, out string? value))
            {
                sb.Append($"${output}: {value}");
            } 
            else
            {
                sb.Append($"${output}: {output} is not defined");
            }            
        }

        return sb.ToString();
    }

    private static List<StepSummary> DeserializeStepsTaken(string json)
    {
        // We can't just deserialize this because the JSON contains a nested dynamic dictionary for action variables.
        // As a result, we're semi-manually deserializing it with JObject and JArray so we can map the action variables.
        JArray items = JArray.Parse(json);
        List<StepSummary> summaries = new();

        foreach (JToken item in items)
        {
            StepSummary summary = new();

            // Action variables is effectively a dictionary of string keys and string values. Newtonsoft can't easily handle it.
            if (item["action_variables"] != null)
            {
                foreach (JProperty variable in item["action_variables"]!)
                {
                    summary.ActionVariables[variable.Name] = variable.Value?.ToString();
                }
            }

            // Simple deserialization of remaining fields
            summary.Action = DeserializeStringValue(item, "action");
            summary.Thought = DeserializeStringValue(item, "thought");
            summary.Observation = DeserializeStringValue(item, "observation");
            summary.FinalAnswer = DeserializeStringValue(item, "final_answer");
            summary.OriginalResponse = DeserializeStringValue(item, "original_response");

            summaries.Add(summary);
        }

        return summaries;
    }

    private static string? DeserializeStringValue(JToken item, string attributeName)
    {
        if (item[attributeName] == null)
        {
            return null;
        }
        return item[attributeName]!.Value<string>();
    }
}
