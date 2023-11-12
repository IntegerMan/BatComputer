using MattEland.BatComputer.Kernel;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
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

        SelectionPrompt<string> choices = new SelectionPrompt<string>()
                                                        .Title("Select an action")
                                                        .AddChoices([ChoiceTypeAQuestion, ChoiceQuit]);

        string choice;
        do
        {
            choice = AnsiConsole.Prompt(choices);

            switch (choice)
            {
                case ChoiceTypeAQuestion:
                    await MakeChatRequestAsync(batKernel);
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

    private async Task MakeChatRequestAsync(BatKernel batKernel)
    {
        string userText = AnsiConsole.Ask<string>($"[{Skin.NormalStyle}]Enter a question:[/]");

        Plan plan = await GeneratePlanAsync(batKernel, userText);
        DisplayPlanTree(plan);
        DisplayJson(plan);

        KernelResult result = await ExecutePlanAsync(batKernel, plan);
        DisplayJson(result);

        AnsiConsole.MarkupLine($"[{Skin.AgentStyle}]{Markup.Escape(result.GetValue<string>() ?? "")}[/]");
        AnsiConsole.WriteLine();
    }

    private static async Task<KernelResult> ExecutePlanAsync(BatKernel batKernel, Plan? plan)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.WriteLine("Executing plan...");

        List<ProgressTask> tasks = new();
        KernelResult? result = null;

        await AnsiConsole.Progress()
            .AutoClear(false)
            .HideCompleted(false)
            .StartAsync(async ctx =>
            {
                foreach (Plan step in plan!.Steps)
                {
                    string taskName = $"{step.Name}: {string.Join(", ", step.Parameters.Select(p => p.Key))}";
                    ProgressTask task = ctx.AddTask(taskName, new ProgressTaskSettings() { MaxValue = 100, AutoStart = true });
                    task.IsIndeterminate(true);
                    tasks.Add(task);
                }

                result = await batKernel.Kernel.RunAsync(plan);

                foreach (ProgressTask task in tasks)
                {
                    task.Increment(100);
                    task.StopTask();
                }

            });
        return result!;
    }

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

    private static void DisplayPlanTree(Plan plan)
    {
        Tree planTree = new("Plan");

        TreeNode state = planTree.AddNode("State");
        foreach (KeyValuePair<string, string> kvp in plan.State)
        {
            state.AddNode($"{kvp.Key}: {kvp.Value}");
        }

        TreeNode steps = planTree.AddNode("Steps");
        foreach (var step in plan.Steps)
        {
            var stepNode = steps.AddNode(step.Name);

            foreach (KeyValuePair<string, string> param in step.Parameters)
            {
                stepNode.AddNode(param.Key + " -->");
            }
            foreach (string output in step.Outputs)
            {
                stepNode.AddNode("--> " + output);
            }

        }

        TreeNode outputs = planTree.AddNode("Outputs");
        foreach (string output in plan.Outputs)
        {
            outputs.AddNode(output);
        }

        AnsiConsole.WriteLine();
        AnsiConsole.Write(planTree);
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
