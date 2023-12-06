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
        try
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[{Skin.NormalStyle}]Executing…[/]");
            AnsiConsole.WriteLine();
            PlanExecutionResult result = await _kernel.ExecuteAsync(prompt);
            _app.RenderTokenUsage();

            return result.Output;
        }
        catch (SKException ex)
        {
            // It's possible to reach the limits of what's possible with the planner. When that happens, handle it gracefully
            if (ex.Message.Contains("Not possible to create plan for goal", StringComparison.OrdinalIgnoreCase) ||
                ex.Message.Contains("Unable to create plan for goal with available functions", StringComparison.OrdinalIgnoreCase))
            {
                Skin.WriteErrorLine("It was not possible to fulfill this request with the available skills.");
            }
            else if (ex.Message.Contains("History is too long", StringComparison.OrdinalIgnoreCase))
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
        catch (InvalidCastException ex)
        {
            // Invalid Cast can happen with llamaSharp
            Skin.WriteException(ex);
            Skin.WriteErrorLine("Could not generate a plan.");

            return null;
        }
        catch (Exception ex)
        {
            Skin.WriteException(ex);
        }

        return null;
    }
}
