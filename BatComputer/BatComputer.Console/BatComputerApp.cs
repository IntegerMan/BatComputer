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

        AnsiConsole.Status().Start("Loading Configuration", LoadSettings);

        BatKernel batKernel = new(_settings);
        DisplayLoadedKernelPlugins(batKernel, showTable: false);

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
                    DisplayLoadedKernelPlugins(batKernel, showTable: true);
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

    private void DisplayLoadedKernelPlugins(BatKernel batKernel, bool showTable)
    {
        List<FunctionView> funcs = batKernel.Kernel.Functions.GetFunctionViews()
                                                             .Where(f => !batKernel.IsFunctionExcluded(f))
                                                             .OrderBy(f => f.PluginName)
                                                             .ThenBy(f => f.Name)
                                                             .ToList();

        string headerMarker = $"[{Skin.SuccessStyle}]{funcs.Count} Plugin Functions Detected[/]";

        if (!showTable)
        {
            IOrderedEnumerable<IGrouping<string, FunctionView>> funcsByPlugin =
                funcs.GroupBy(f => f.PluginName)
                     .OrderByDescending(g => g.Count())
                     .ThenBy(g => g.Key);

            AnsiConsole.Write(new BarChart()
                .Label(headerMarker)
                .LeftAlignLabel()
                .AddItems(funcsByPlugin, item => new BarChartItem(item.Key, item.Count()))); // TODO: Colorize by value
        }
        else
        {
            AnsiConsole.MarkupLine(headerMarker);

            Table funcTable = new();
            funcTable.AddColumns("Name", "Parameters", "Description");
            foreach (FunctionView funcView in funcs)
            {
                string parameters = string.Join(", ", funcView.Parameters.Select(p =>
                {
                    StringBuilder sb = new(p.Name);

                    if (p.IsRequired != true)
                    {
                        sb.Append('?');
                    }

                    if (!string.IsNullOrEmpty(p.DefaultValue))
                    {
                        sb.Append(" = " + p.DefaultValue);
                    }

                    return sb.ToString();
                }));
                string qualifiedName = $"[{Skin.NormalStyle}]{funcView.PluginName}[/]:[{Skin.AccentStyle}]{funcView.Name}[/]";

                funcTable.AddRow(qualifiedName, parameters, funcView.Description);
            }
            AnsiConsole.Write(funcTable);
        }

        AnsiConsole.WriteLine();
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

        DisplayPlanTree(plan);

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
            AnsiConsole.MarkupLine($"[{Skin.NormalStyle}]Target Variable: [/][{Skin.AccentStyle}]{GetPlanTarget(plan)}[/]");
        }
    }

    private static string GetResponse(Plan plan)
    {
        string targetKey = GetPlanTarget(plan);

        const string defaultResponse = "I'm sorry, but I was not able to generate a response.";

        if (plan.State.TryGetValue(targetKey, out string? response))
        {
            return response ?? defaultResponse;
        }

        return plan.State.Values.FirstOrDefault(defaultValue: defaultResponse);
    }

    private static string GetPlanTarget(Plan plan)
    {
        string targetKey;
        if (plan.Outputs.Count == 0)
        {
            // We shouldn't have had a plan with no outputs, but this is a good key to use just in case
            targetKey = "RESULT__RESPONSE";
        }
        else
        {
            // Either we have 1 or more than 1 output variable. In either case, let's go with the last entry as the most likely usable one
            targetKey = plan.Outputs.Last();
        }

        return targetKey;
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

    private void DisplayPlanTree(Plan plan)
    {
        Tree planTree = new($"[{Skin.NormalStyle}]{plan.PluginName}[/]:[{Skin.AccentStyle}]{plan.Name}[/]");

        string target = GetPlanTarget(plan);

        PopulateTree(plan, planTree, target);

        AnsiConsole.Write(planTree);
        AnsiConsole.WriteLine();
    }

    private void PopulateTree(Plan plan, IHasTreeNodes tree, string target)
    {
        if (plan.Parameters.Any())
        {
            TreeNode parameters = tree.AddNode("Parameters");
            foreach (KeyValuePair<string, string> param in plan.Parameters)
            {
                parameters.AddNode($":incoming_envelope: [{Skin.NormalStyle}]{param.Key}[/]");
            }
        }

        if (plan.Steps.Any())
        {
            TreeNode steps = tree.AddNode("Steps");
            foreach (Plan step in plan.Steps)
            {
                TreeNode stepNode = steps.AddNode($"[{Skin.NormalStyle}]{step.Name}[/]");

                PopulateTree(step, stepNode, target);
            }
        }

        if (plan.State.Any())
        {
            TreeNode state = tree.AddNode("State");

            foreach (KeyValuePair<string, string> kvp in plan.State)
            {
                // Plan.Result tends to be every other answer joined together, so displaying it is usually meaningless / noisy
                if (kvp.Key == "PLAN.RESULT")
                    continue;

                if (kvp.Key == target)
                {
                    state.AddNode($"[{Skin.AccentStyle}]{kvp.Key}[/]:[{Skin.NormalStyle}]{kvp.Value}[/]");
                }
                else
                {
                    state.AddNode($"[{Skin.NormalStyle}]{kvp.Key}[/]:[{Skin.DebugStyle}]{kvp.Value}[/]");
                }
            }
        }

        if (plan.Outputs.Any())
        {
            TreeNode outputs = tree.AddNode("Outputs");
            foreach (string output in plan.Outputs)
            {
                if (output == target)
                {
                    outputs.AddNode($":goal_net: [{Skin.AccentStyle}]{output}[/]");
                }
                else
                {
                    outputs.AddNode($":outbox_tray: [{Skin.NormalStyle}]{output}[/]");
                }
            }
        }
    }

    private static void DisplayJson(object? input)
    {
        string json = JsonSerializer.Serialize(input);
        AnsiConsole.Write(new JsonText(json));
        AnsiConsole.WriteLine();
    }

    private void LoadSettings(StatusContext ctx)
    {
        ctx.Spinner(Skin.Spinner);

        // Load settings
        IConfigurationRoot config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddUserSecrets(GetType().Assembly)
            .AddEnvironmentVariables()
            .AddCommandLine(Environment.GetCommandLineArgs())
            .Build();

        AnsiConsole.MarkupLine($"[{Skin.NormalStyle}]Reading configuration data[/]");
        ReadRequiredSetting(config, "AzureAIEndpoint", v => _settings.AzureAiServicesEndpoint = v);
        ReadRequiredSetting(config, "AzureAIKey", v => _settings.AzureAiServicesKey = v);
        ReadRequiredSetting(config, "AzureOpenAIEndpoint", v => _settings.AzureOpenAiEndpoint = v);
        ReadRequiredSetting(config, "AzureOpenAIKey", v => _settings.AzureOpenAiKey = v);
        ReadRequiredSetting(config, "OpenAIDeploymentName", v => _settings.OpenAiDeploymentName = v);

        ctx.Status("Validating settings");
        AnsiConsole.WriteLine();

        if (!_settings.IsValid)
        {
            StringBuilder sb = new();
            sb.AppendLine("Settings were in an invalid state after all settings were parsed:");
            foreach (System.ComponentModel.DataAnnotations.ValidationResult violation in _settings.Validate(new ValidationContext(_settings)))
            {
                sb.AppendLine($"- {violation.ErrorMessage}");
            }
            throw new InvalidOperationException(sb.ToString());
        }

        AnsiConsole.MarkupLine($"[{Skin.NormalStyle}]Finished reading config settings[/]");
        AnsiConsole.WriteLine();
    }

    private void ReadRequiredSetting(IConfigurationRoot config, string settingName, Action<string> applyAction)
    {
        string? value = config[settingName];
        if (string.IsNullOrEmpty(value))
        {
            AnsiConsole.MarkupLine($"[{Skin.ErrorStyle}]{Skin.ErrorEmoji} Could not read the config value for {settingName}[/]");
        }
        else
        {
            applyAction(value);
            AnsiConsole.MarkupLine($"[{Skin.SuccessStyle}]{Skin.SuccessEmoji} Read setting {settingName}[/]");
        }
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
