using MattEland.BatComputer.Abstractions;
using MattEland.BatComputer.ConsoleApp.Abstractions;
using MattEland.BatComputer.Kernel;
using Microsoft.SemanticKernel.Diagnostics;
using Microsoft.SemanticKernel.Planning;
using Spectre.Console;
using Microsoft.SemanticKernel;

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

    public async Task<string> GetKernelPromptResponseAsync(string prompt)
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
                AnsiConsole.MarkupLine($"[{Skin.ErrorStyle}]{Skin.ErrorEmoji} Not possible to create plan for goal with available functions. Sending as chat request.[/]");
            }
            else
            {
                // Not an impossible plan. Display additional details
                AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);
                AnsiConsole.MarkupLine($"[{Skin.ErrorStyle}]{Skin.ErrorEmoji} Could not generate a plan. Will send as a chat request without planning.[/]");
            }

            // Fallback to handling via chat request
            return await _kernel.GetChatPromptResponseAsync(prompt);
        }
        catch (InvalidCastException ex)
        {
            // Invalid Cast can happen with llamaSharp
            AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);
            AnsiConsole.MarkupLine($"[{Skin.ErrorStyle}]{Skin.ErrorEmoji} Could not generate a plan. Will send as a chat request without planning.[/]");

            // Fallback to handling via chat request
            return await _kernel.GetChatPromptResponseAsync(prompt);
        }

        // Show the user a plan before we execute it
        DisplayPlanDescription(plan);

        // ExecuteAsync the plan step by step and show progress
        try
        {
            await ExecutePlanAsync(plan);
        }
        catch (HttpOperationException ex)
        {
            if (ex.Message.Contains("content management policy", StringComparison.OrdinalIgnoreCase))
            {
                AnsiConsole.MarkupLine($"[{Skin.ErrorStyle}]{Skin.ErrorEmoji} The message or its response was flagged for inappropriate content and could not be processed.[/]");
            } 
            else
            {
                AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);
                AnsiConsole.MarkupLine($"[{Skin.ErrorStyle}]{Skin.ErrorEmoji} {Markup.Escape(ex.Message)}.[/]");
            }
        }

        // Get the response from the plan
        return GetResponse(plan);
    }

    private void DisplayPlanDescription(Plan plan, bool displayTargetVariable = false)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[{Skin.SuccessStyle}]Generated plan:[/]");
        foreach (Plan step in plan.Steps)
        {
            AnsiConsole.MarkupLine($"[{Skin.NormalStyle}] - {step.Name}[/]");
        }

        if (displayTargetVariable)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[{Skin.NormalStyle}]Target Variable: [/][{Skin.AccentStyle}]{plan.GetTarget()}[/]");
        }
    }

    private static string GetResponse(Plan plan)
    {
        if (plan.State.TryGetValue(plan.GetTarget(), out string? response))
        {
            return response;
        }

        return plan.State.Values.FirstOrDefault(defaultValue: "I'm sorry, but I was not able to generate a response.");
    }

    private async Task ExecutePlanAsync(Plan plan)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.WriteLine("Executing plan...");

        List<ProgressTask> tasks = new();

        await AnsiConsole.Status().StartAsync("Executing...", async ctx =>
        {
            ctx.Spinner = Skin.Spinner;

            await plan.InvokeAsync(_kernel.Kernel);
        });

        AnsiConsole.WriteLine("Done");

        /*
await AnsiConsole.Progress()
    .AutoClear(false)
    .HideCompleted(false)
    .StartAsync(async ctx =>
    {
        // SKContext result = await appKernel.Planner.ExecutePlanAsync(appKernel.LastGoal, appKernel.Kernel.CreateNewContext());

        // Register the tasks we'll be accomplishing so the user can see them in order
        foreach (Plan step in plan.Steps)
        {
            ProgressTask task = ctx.AddTask(step.Name, new ProgressTaskSettings() { MaxValue = 100, AutoStart = false });
            tasks.Add(task);
        }

        // Sequentially execute each step
        foreach (ProgressTask task in tasks)
        {
            // Have the UI show it as in progress
            task.StartTask();
            task.IsIndeterminate();

            // ExecuteAsync the step
            await _kernel.Kernel.StepAsync(plan);

            // Complete it in the UI
            task.Increment(100);
            task.StopTask();
        }
    });
        */
    }


    /// <summary>
    /// Builds a kernel execution plan from the provided text
    /// </summary>
    /// <param name="userText">The prompt the user typed in</param>
    /// <returns>The generated plan</returns>
    /// <exception cref="SKException">Thrown when a plan could not be generated</exception>
    private async Task<Plan> GeneratePlanAsync(string userText)
    {
        AnsiConsole.WriteLine("Generating plan...");

        Plan? plan = null;
        await AnsiConsole.Status().StartAsync("Planning...", async ctx =>
        {
            ctx.Spinner(Skin.Spinner);

            plan = await _kernel.PlanAsync(userText);
        });

        return plan!;
    }
}
