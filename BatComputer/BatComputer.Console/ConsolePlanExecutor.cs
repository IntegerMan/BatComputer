using MattEland.BatComputer.ConsoleApp.Abstractions;
using MattEland.BatComputer.Kernel;
using Microsoft.SemanticKernel.Diagnostics;
using Microsoft.SemanticKernel.Planning;
using Spectre.Console;
using Microsoft.SemanticKernel;
using MattEland.BatComputer.ConsoleApp.Helpers;
using Microsoft.SemanticKernel.Orchestration;
using MattEland.BatComputer.Abstractions.Strategies;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MattEland.BatComputer.ConsoleApp;

public class ConsolePlanExecutor
{
    private readonly BatComputerApp _app;
    private readonly AppKernel _kernel;

    public ConsolePlanExecutor(BatComputerApp app, AppKernel kernel)
    {
        _app = app;
        _kernel = kernel;
    }

    protected ConsoleSkin Skin => _app.Skin;

    public async Task<string?> GetKernelPromptResponseAsync(string prompt)
    {
        Plan plan;
        try
        {
            plan = await GeneratePlanAsync(prompt);
        }
        catch (SKException ex)
        {
            // It's possible to reach the limits of what's possible with the planner. When that happens, handle it gracefully
            if (ex.Message.Contains("Not possible to create plan for goal", StringComparison.OrdinalIgnoreCase) ||
                ex.Message.Contains("Unable to create plan for goal with available functions", StringComparison.OrdinalIgnoreCase))
            {
                Skin.WriteErrorLine("It was not possible to fulfill this request with the available skills. Will send as a chat request without planning.");                
            }
            else
            {
                // Not an impossible plan. Display additional details
                Skin.WriteException(ex);
                Skin.WriteErrorLine("Could not generate a plan. Will send as a chat request without planning.");
            }

            // Fallback to handling via chat request
            return await _kernel.GetChatPromptResponseAsync(prompt);
        }
        catch (InvalidCastException ex)
        {
            // Invalid Cast can happen with llamaSharp
            Skin.WriteException(ex);
            Skin.WriteErrorLine("Could not generate a plan. Will send as a chat request without planning.");

            // Fallback to handling via chat request
            return await _kernel.GetChatPromptResponseAsync(prompt);
        }

        // Show the user a plan before we execute it
        DisplayPlanDescription(plan);

        try
        {
            PlanExecutionResult result = await ExecutePlanAsync(plan);
            // result.Dump();

            return result.Output;
        }
        catch (SKException ex)
        {
            if (ex.Message.Contains("History is too long", StringComparison.OrdinalIgnoreCase))
            {
                Skin.WriteErrorLine("The planner caused too many tokens to be used to fulfill the request. There may be too many functions enabled.");
            } 
            else
            {
                Skin.WriteException(ex);
            }
        }
        catch (HttpOperationException ex)
        {
            if (ex.Message.Contains("content management policy", StringComparison.OrdinalIgnoreCase))
            {
                Skin.WriteErrorLine("The message or its response was flagged for inappropriate content and could not be processed");
            } 
            else
            {
                Skin.WriteException(ex);
            }
        }
        catch (Exception ex)
        {
            Skin.WriteException(ex);
        }

        return null;
    }

    private void DisplayPlanDescription(Plan plan)
    {
        if (plan.Steps.Any())
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[{Skin.SuccessStyle}]Generated plan:[/]");

            foreach (Plan step in plan.Steps)
            {
                AnsiConsole.MarkupLine($"[{Skin.NormalStyle}] - {step.Name}[/]");
            }
        }
    }

    /* TODO
    private static string GetResponse(Plan plan)
    {
        if (plan.State.TryGetValue(plan.GetTarget(), out string? response))
        {
            return response;
        }

        return plan.State.Values.FirstOrDefault(defaultValue: "I'm sorry, but I was not able to generate a response.");
    }
    */

    private async Task<PlanExecutionResult> ExecutePlanAsync(Plan plan)
    {
        PlanExecutionResult executionResult = new();
        FunctionResult? result = null;

        await AnsiConsole.Status().StartAsync("Executing...", async ctx =>
        {
            ctx.Spinner = Skin.Spinner;

            result = await plan.InvokeAsync(_kernel.Kernel);
        });

        PopulateExecutionResult(executionResult, result);

        _kernel.LastResult = executionResult;

        return executionResult;
    }

    private static void PopulateExecutionResult(PlanExecutionResult executionResult, FunctionResult? result)
    {
        if (result == null)
        {
            return;
        }

        if (result.TryGetMetadataValue("stepCount", out string stepCount))
        {
            executionResult.StepsCount = int.Parse(stepCount);
        }

        if (result.TryGetMetadataValue("stepsTaken", out string json))
        {
            JArray items = JArray.Parse(json);

            List<StepwiseSummary> summaries = new();

            foreach (JToken item in items)
            {
                StepwiseSummary summary = new();

                if (item["action"] != null)
                {
                    summary.Action = item["action"]!.Value<string>();
                }

                if (item["thought"] != null)
                {
                    summary.Thought = item["thought"]!.Value<string>();
                }

                if (item["observation"] != null)
                {
                    summary.Observation = item["observation"]!.Value<string>();
                }

                if (item["action_variables"] != null)
                {
                    foreach (JProperty variable in item["action_variables"]!)
                    {
                        summary.ActionVariables[variable.Name] = variable.Value?.ToString();
                    }
                }

                if (item["final_answer"] != null)
                {
                    summary.FinalAnswer = item["final_answer"]!.Value<string>();
                }

                if (item["original_response"] != null)
                {
                    summary.OriginalResponse = item["original_response"]!.Value<string>();
                }

                summaries.Add(summary);
            }
            // get JSON result objects into a list
            //IList<JToken> results = googleSearch["responseData"]["results"].Children().ToList();

            executionResult.Summary = summaries;
        }

        if (result.TryGetMetadataValue("functionCount", out string functionCount))
        {
            executionResult.FunctionsUsed = functionCount;
        }

        if (result.TryGetMetadataValue("iterations", out string iterations))
        {
            executionResult.Iterations = int.Parse(iterations);
        }

        executionResult.Output = result.GetValue<string>();
    }


    /// <summary>
    /// Builds a kernel execution plan from the provided text
    /// </summary>
    /// <param name="userText">The prompt the user typed in</param>
    /// <returns>The generated plan</returns>
    /// <exception cref="SKException">Thrown when a plan could not be generated</exception>
    private async Task<Plan> GeneratePlanAsync(string userText)
    {
        Plan? plan = null;
        await AnsiConsole.Status().StartAsync("Planning...", async ctx =>
        {
            ctx.Spinner(Skin.Spinner);

            plan = await _kernel.PlanAsync(userText);
        });

        return plan!;
    }
}
