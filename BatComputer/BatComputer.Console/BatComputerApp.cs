using BatComputer.Abstractions;
using BatComputer.Skins;
using Dumpify;
using MattEland.BatComputer.Abstractions;
using MattEland.BatComputer.Kernel;
using MattEland.BatComputer.ConsoleApp.Helpers;
using MattEland.BatComputer.ConsoleApp.Renderables;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Diagnostics;
using Microsoft.SemanticKernel.Planning;
using Spectre.Console;

namespace MattEland.BatComputer.ConsoleApp;

public class BatComputerApp
{
    private readonly KernelSettings _settings = new();
    public ConsoleSkin Skin { get; set; } = new BatComputerSkin();

    public async Task<int> RunAsync()
    {
        WelcomeRenderer.ShowWelcome(Skin);

        LoadSettings();

        AppKernel appKernel = new(_settings);
        appKernel.RenderKernelPluginsChart(Skin);

        await RunMainLoopAsync(appKernel);

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[{Skin.SuccessStyle}]Program complete[/]");

        return 0;
    }

    private async Task RunMainLoopAsync(AppKernel appKernel)
    {
        MainMenuOption choice;
        do
        {
            // TODO: This probably will need hierarchical sub-menus as options grow
            SelectionPrompt<MainMenuOption> choices = new SelectionPrompt<MainMenuOption>()
                    .Title($"[{Skin.NormalStyle}]Select an action[/]")
                    .HighlightStyle(Skin.AccentStyle)
                    .AddChoices(GetMainMenuOptions(appKernel))
                    .UseConverter(c => c switch
                    {
                        MainMenuOption.Query => $"Query {Skin.AppNamePrefix} {Skin.AppName}",
                        MainMenuOption.Chat => $"Chat with {Skin.AgentName}",
                        _ => c.ToFriendlyString()
                    });

            // TODO: This probably will need the command pattern
            choice = AnsiConsole.Prompt(choices);
            switch (choice)
            {
                case MainMenuOption.Query:
                    {
                        string prompt = InputHelpers.GetUserText($"[{Skin.NormalStyle}]Type your request:[/]");
                        if (!string.IsNullOrWhiteSpace(prompt))
                        {
                            string response = await GetKernelPromptResponseAsync(appKernel, prompt);

                            DisplayChatResponse(appKernel, response);
                        }
                    }
                    break;
                case MainMenuOption.Chat:
                    {
                        string prompt = InputHelpers.GetUserText($"[{Skin.NormalStyle}]Type your message:[/]");
                        if (!string.IsNullOrWhiteSpace(prompt))
                        {
                            string chatResponse = await GetChatPromptResponseAsync(appKernel, prompt);

                            DisplayChatResponse(appKernel, chatResponse);
                        }
                    }
                    break;
                case MainMenuOption.RetryLastMessage:
                    {
                        string response = await GetKernelPromptResponseAsync(appKernel, appKernel.LastMessage!);

                        DisplayChatResponse(appKernel, response);
                    }
                    break;
                case MainMenuOption.ShowPlanTree:
                    appKernel.LastPlan!.RenderTree(Skin);
                    break;
                case MainMenuOption.ShowPlanJson:
                    appKernel.LastPlan!.RenderJson();
                    break;
                case MainMenuOption.ListPlugins:
                    appKernel.RenderKernelPluginsTable(Skin);
                    break;
                case MainMenuOption.Quit:
                    AnsiConsole.MarkupLine($"[{Skin.NormalStyle}]Shutting down[/]");
                    break;
                default:
                    throw new NotSupportedException($"Choice {choice} was not implemented in {nameof(RunMainLoopAsync)}");
            }
        } while (choice != MainMenuOption.Quit);
    }

    private static IEnumerable<MainMenuOption> GetMainMenuOptions(AppKernel appKernel)
    {
        if (appKernel.HasPlanner)
        {
            yield return MainMenuOption.Query;
        }

        yield return MainMenuOption.Chat;
        yield return MainMenuOption.ListPlugins;

        if (!string.IsNullOrWhiteSpace(appKernel.LastMessage))
        {
            yield return MainMenuOption.RetryLastMessage;
        }

        if (appKernel.LastPlan != null)
        {
            yield return MainMenuOption.ShowPlanTree;
            yield return MainMenuOption.ShowPlanJson;
        }

        yield return MainMenuOption.Quit;
    }

    private void DisplayChatResponse(IAppKernel kernel, string chatResponse)
    {
        while (kernel.Widgets.Any())
        {
            IWidget widget = kernel.Widgets.Dequeue();
            widget.Dump(label: widget.ToString(),
                typeNames: new TypeNamingConfig { ShowTypeNames = false }, 
                tableConfig: new TableConfig { ShowTableHeaders = false }, 
                members: new MembersConfig { IncludeFields = false, IncludeNonPublicMembers = false });
        }

        AnsiConsole.MarkupLine($"[{Skin.AgentStyle}]{Markup.Escape(Skin.AgentName)}: {Markup.Escape(chatResponse)}[/]");
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

    private async Task<string> GetKernelPromptResponseAsync(AppKernel appKernel, string prompt)
    {
        Plan plan;
        try
        {
            plan = await GeneratePlanAsync(appKernel, prompt);
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
                AnsiConsole.WriteException(ex);
                AnsiConsole.MarkupLine($"[{Skin.ErrorStyle}]{Skin.ErrorEmoji} Could not generate a plan. Will send as a chat request without planning.[/]");
            }

            // Fallback to handling via chat request
            return await GetChatPromptResponseAsync(appKernel, prompt);
        }
        catch (InvalidCastException ex)
        {
            // Invalid Cast can happen with llamaSharp
            AnsiConsole.WriteException(ex);
            AnsiConsole.MarkupLine($"[{Skin.ErrorStyle}]{Skin.ErrorEmoji} Could not generate a plan. Will send as a chat request without planning.[/]");

            // Fallback to handling via chat request
            return await GetChatPromptResponseAsync(appKernel, prompt);
        }

        // Show the user a plan before we execute it
        DisplayPlanDescription(plan);

        // Execute the plan step by step and show progress
        await ExecutePlanAsync(appKernel, plan);

        // Get the response from the plan
        return GetResponse(plan);
    }

    private async Task<string> GetChatPromptResponseAsync(AppKernel appKernel, string prompt)
    {
        try
        {
            return await appKernel.GetChatResponseAsync(prompt);
        }
        catch (HttpOperationException ex)
        {
            if (ex.Message.Contains("does not work with the specified model"))
            {
                return $"[{Skin.ErrorStyle}]Your model does not support the current option. You may be trying to use a completion model with a chat feature or vice versa. Try using a different deployment model.[/]";
            }
            throw;
        }
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

    private static async Task ExecutePlanAsync(AppKernel appKernel, Plan plan)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.WriteLine("Executing plan...");

        List<ProgressTask> tasks = new();

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
                    task.IsIndeterminate(true);

                    // Execute the step
                    await appKernel.Kernel.StepAsync(plan);

                    // Complete it in the UI
                    task.Increment(100);
                    task.StopTask();
                }
            });
    }

    /// <summary>
    /// Builds a kernel execution plan from the provided text
    /// </summary>
    /// <param name="appKernel">The main kernel wrapper</param>
    /// <param name="userText">The prompt the user typed in</param>
    /// <returns>The generated plan</returns>
    /// <exception cref="SKException">Thrown when a plan could not be generated</exception>
    private async Task<Plan> GeneratePlanAsync(AppKernel appKernel, string userText)
    {
        AnsiConsole.WriteLine("Generating plan...");

        Plan? plan = null;
        await AnsiConsole.Status().StartAsync("Planning...", async ctx =>
        {
            ctx.Spinner(Skin.Spinner);

            plan = await appKernel.PlanAsync(userText);
        });

        return plan!;
    }
}
