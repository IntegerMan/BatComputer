using MattEland.BatComputer.ConsoleApp.Abstractions;
using MattEland.BatComputer.Kernel;
using Microsoft.SemanticKernel.Diagnostics;
using Microsoft.SemanticKernel.Planning;
using Spectre.Console;
using MattEland.BatComputer.ConsoleApp.Helpers;
using MattEland.BatComputer.Abstractions.Strategies;

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
            await AnsiConsole.Status().StartAsync("Planning...", async ctx =>
            {
                ctx.Spinner(Skin.Spinner);

                plan = await _kernel.PlanAsync(prompt);
            });
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

        try
        {
            PlanExecutionResult result = new();

            await AnsiConsole.Status().StartAsync("Executing...", async ctx =>
            {
                ctx.Spinner = Skin.Spinner;

                result = await _kernel.ExecutePlanAsync();
            });
            // result.Dump();

            return result.Output;
        }
        catch (SKException ex)
        {
            if (ex.Message.Contains("History is too long", StringComparison.OrdinalIgnoreCase))
            {
                Skin.WriteErrorLine("The planner caused too many tokens to be used to fulfill the request. There may be too many functions enabled.");
            } 
            else if (ex.Message.Contains("Missing value for parameter"))
            {
                Skin.WriteErrorLine(ex.Message);
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
}
