using MattEland.BatComputer.ConsoleApp.Renderables;
using MattEland.BatComputer.Kernel;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Diagnostics;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.Planning;
using Spectre.Console;
using Spectre.Console.Json;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json;

namespace MattEland.BatComputer.ConsoleApp;

public class BatComputerApp
{
    private const string ChoiceTypeAQuestion = "Type in a question";
    private const string ChoiceListPlugins = "List plugin functions";
    private const string ChoiceQuit = "Quit";

    private readonly BatComputerSettings _settings = new();
    public ConsoleSkin Skin { get; set; } = new BatComputerSkin();

    public async Task<int> RunAsync()
    {
        // Stylize the app
        Color colorPrimary = Skin.NormalColor;
        Style primary = new(foreground: colorPrimary);
        AnsiConsole.Foreground = colorPrimary;

        // Show the main welcome header
        ShowWelcomeMenu(colorPrimary, primary);

        LoadSettings();

        BatKernel batKernel = new(_settings);
        batKernel.RenderKernelPluginsChart(Skin);

        SelectionPrompt<string> choices = new SelectionPrompt<string>()
                                                        .Title("Select an action")
                                                        .AddChoices([ChoiceTypeAQuestion, ChoiceListPlugins, ChoiceQuit]);

        string choice;
        do
        {
            choice = AnsiConsole.Prompt(choices);

            switch (choice)
            {
                case ChoiceTypeAQuestion:
                    await MakeChatRequestAsync(batKernel);
                    break;

                case ChoiceListPlugins:
                    batKernel.RenderKernelPluginsTable(Skin);
                    break;

                case ChoiceQuit:
                    AnsiConsole.MarkupLine($"[{Skin.NormalStyle}]Shutting down[/]");
                    break;
            }
        } while (choice != ChoiceQuit);

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[{Skin.SuccessStyle}]Program complete[/]");

        return 0;
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

    private async Task MakeChatRequestAsync(BatKernel batKernel)
    {
        string userText = AnsiConsole.Ask<string>($"[{Skin.NormalStyle}]Enter a question:[/]");
        Plan plan;
        try
        {
            plan = await GeneratePlanAsync(batKernel, userText);
        }
        catch (SKException ex)
        {
            AnsiConsole.WriteException(ex);

            AnsiConsole.MarkupLine($"[{Skin.ErrorStyle}]{Skin.ErrorEmoji} Could not generate a plan. Will send the request on for chat response without planning.[/]");
            // TODO: Actually do this

            return;
        }

        DisplayPlanDescription(plan);

        await ExecutePlanAsync(batKernel, plan);

        plan.RenderTree(Skin);

        string response = GetResponse(plan);

        AnsiConsole.MarkupLine($"[{Skin.AgentStyle}]{Skin.AgentName}: {response}[/]");
        AnsiConsole.WriteLine();
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

                // Sequentially execute each sstep
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

            string goal = $"User: {userText}" + """

---------------------------------------------

Respond to this statement.
""";
            plan = await batKernel.Planner.CreatePlanAsync(userText);
        });

        return plan!;
    }

    private static void DisplayJson(object? input)
    {
        string json = JsonSerializer.Serialize(input);
        AnsiConsole.Write(new JsonText(json));
        AnsiConsole.WriteLine();
    }

    private void ShowWelcomeMenu(Color colorPrimary, Style primary)
    {
        AnsiConsole.WriteLine($"Welcome to {Skin.AppNamePrefix}".TrimEnd());

        AnsiConsole.Write(new FigletText(FigletFont.Default, Skin.AppName)
            .Centered()
            .Color(colorPrimary));

        Version version = GetType().Assembly.GetName().Version!;

        AnsiConsole.Write(new Text($"Version {version}", style: primary).RightJustified());
        AnsiConsole.WriteLine();

        AnsiConsole.WriteLine();
    }
}
