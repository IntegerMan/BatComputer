using MattEland.BatComputer.ConsoleApp.Renderables;
using MattEland.BatComputer.Kernel;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Diagnostics;
using Microsoft.SemanticKernel.Planning;
using Spectre.Console;
using MattEland.BatComputer.ConsoleApp.Skins;

namespace MattEland.BatComputer.ConsoleApp;

public class BatComputerApp
{
    private readonly BatComputerSettings _settings = new();
    public ConsoleSkin Skin { get; set; } = new BatComputerSkin();

    public async Task<int> RunAsync()
    {
        WelcomeRenderer.ShowWelcome(Skin);

        LoadSettings();

        BatKernel batKernel = new(_settings);
        batKernel.RenderKernelPluginsChart(Skin);

        await RunMainLoopAsync(batKernel);

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[{Skin.SuccessStyle}]Program complete[/]");

        return 0;
    }

    private async Task RunMainLoopAsync(BatKernel batKernel)
    {
        string ChoiceKernel = $"Query {Skin.AppNamePrefix} {Skin.AppName}";
        string ChoiceChat = $"Chat with {Skin.AgentName}";
        const string ChoiceListPlugins = "List Plugins";
        const string ChoiceQuit = "Quit";

        List<string> selectionOptions = new()
        {
            ChoiceKernel,
            ChoiceChat,
            ChoiceListPlugins,
            ChoiceQuit
        };

        // TODO: This would allow a switch if we used Enums instead of strings
        SelectionPrompt<string> choices = new SelectionPrompt<string>()
                .Title($"[{Skin.NormalStyle}]Select an action[/]")
                .HighlightStyle(Skin.AccentStyle)
                .AddChoices(selectionOptions);

        string choice;
        do
        {
            choice = AnsiConsole.Prompt(choices);

            if (choice == ChoiceKernel)
            {
                string prompt = GetUserText($"[{Skin.NormalStyle}]Type your request:[/]");

                string response = await GetKernelPromptResponseAsync(batKernel, prompt);

                DisplayChatResponse(response);
            }
            else if (choice == ChoiceChat)
            {
                string chatPrompt = GetUserText($"[{Skin.NormalStyle}]Type your message:[/]");

                string chatResponse = await GetChatPromptResponseAsync(batKernel, chatPrompt);

                DisplayChatResponse(chatResponse);
            }
            else if (choice == ChoiceListPlugins)
            {
                batKernel.RenderKernelPluginsTable(Skin);
            }
            else if (choice == ChoiceQuit)
            {
                AnsiConsole.MarkupLine($"[{Skin.NormalStyle}]Shutting down[/]");
            }
            else
            {
                throw new NotSupportedException($"Choice {choice} was not implemented in {nameof(RunMainLoopAsync)}");
            }
        } while (choice != ChoiceQuit);
    }

    private static string GetUserText(string message)
    {
        string prompt = AnsiConsole.Ask<string>(message);

        AnsiConsole.WriteLine();

        return prompt;
    }

    private void DisplayChatResponse(string chatResponse)
    {
        AnsiConsole.MarkupLine($"[{Skin.AgentStyle}]{Skin.AgentName}: {chatResponse}[/]");
        AnsiConsole.WriteLine();
    }

    private void LoadSettings()
    {
        AnsiConsole.Status().Start("Loading Configuration", ctx =>
        {
            ctx.Spinner(Skin.Spinner);

            _settings.Load(Skin);

            ctx.Status("Validating settings");
            AnsiConsole.WriteLine();

            _settings.Validate();
        });
    }

    private async Task<string> GetKernelPromptResponseAsync(BatKernel batKernel, string prompt)
    {
        Plan plan;
        try
        {
            plan = await GeneratePlanAsync(batKernel, prompt);
        }
        catch (SKException ex)
        {
            // It's possible to reach the limits of what's possible with the planner. When that happens, handle it gracefully
            if (ex.Message.Contains("Not possible to create plan for goal", StringComparison.OrdinalIgnoreCase))
            {
                AnsiConsole.MarkupLine($"[{Skin.ErrorStyle}]{Skin.ErrorEmoji} Not possible to create plan for goal with available functions. Sending as chat request.[/]");
            }
            else
            {
                // Not an impossible plan. Display additional details
                AnsiConsole.WriteException(ex);
                AnsiConsole.MarkupLine($"[{Skin.ErrorStyle}]{Skin.ErrorEmoji} Could not generate a plan. Will send as a chat request without planning.[/]");
            }

            // Fallback to handling via chat request
            return await GetChatPromptResponseAsync(batKernel, prompt);
        }

        // Show the user a plan before we execute it
        DisplayPlanDescription(plan);

        // Execute the plan step by step and show progress
        await ExecutePlanAsync(batKernel, plan);

        // Display the final state of the plan
        plan.RenderTree(Skin);

        // Get the response from the plan
        return GetResponse(plan);
    }

    private static async Task<string> GetChatPromptResponseAsync(BatKernel batKernel, string prompt) 
        => await batKernel.GetChatResponseAsync(prompt);

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
        const string defaultResponse = "I'm sorry, but I was not able to generate a response.";

        if (plan.State.TryGetValue(plan.GetTarget(), out string? response))
        {
            return response ?? defaultResponse;
        }

        return plan.State.Values.FirstOrDefault(defaultValue: defaultResponse);
    }

    private static async Task ExecutePlanAsync(BatKernel batKernel, Plan plan)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.WriteLine("Executing plan...");

        List<ProgressTask> tasks = new();

        await AnsiConsole.Progress()
            .AutoClear(false)
            .HideCompleted(false)
            .StartAsync(async ctx =>
            {
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
                    task.IsIndeterminate(true);

                    // Execute the step
                    await batKernel.Kernel.StepAsync(plan);

                    // Complete it in the UI
                    task.Increment(100);
                    task.StopTask();
                }

            });
    }

    /// <summary>
    /// Builds a kernel execution plan from the provided text
    /// </summary>
    /// <param name="batKernel">The main kernel wrapper</param>
    /// <param name="userText">The prompt the user typed in</param>
    /// <returns>The generated plan</returns>
    /// <exception cref="SKException">Thrown when a plan could not be generated</exception>
    private async Task<Plan> GeneratePlanAsync(BatKernel batKernel, string userText)
    {
        AnsiConsole.WriteLine("Generating plan...");

        Plan? plan = null;
        await AnsiConsole.Status().StartAsync("Planning...", async ctx =>
        {
            ctx.Spinner(Skin.Spinner);

            plan = await batKernel.PlanAsync(userText);
        });

        return plan!;
    }
}
