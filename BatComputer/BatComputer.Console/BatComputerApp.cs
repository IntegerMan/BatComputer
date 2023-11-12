using MattEland.BatComputer.Kernel;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.Planners;
using Microsoft.SemanticKernel.Planning;
using Spectre.Console;
using Spectre.Console.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MattEland.BatComputer.ConsoleApp;

public class BatComputerApp
{
    private const string SystemText = "You are an AI assistant named Alfred, the virtual butler to Batman. The user is Batman.";
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

        SpectreLoggerFactory loggerFactory = new(Skin);
        BatKernel batKernel = new(_settings, loggerFactory);

        AnsiConsole.WriteLine();

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
        string userText = AnsiConsole.Ask<string>($"[{Skin.NormalStyle}]Enter a question for OpenAI:[/]");
        AnsiConsole.WriteLine();

        await AnsiConsole.Status().StartAsync("Prompting...", async ctx =>
        {
            ctx.Spinner(Skin.Spinner);

            ctx.Status("Planning requests...");
            Plan plan = await batKernel.Planner.CreatePlanAsync(userText);

            AnsiConsole.WriteLine();

            string json = JsonSerializer.Serialize(plan);
            AnsiConsole.Write(new JsonText(json));
            AnsiConsole.WriteLine();

            ctx.Status("Requesting data...");

            KernelResult result = await batKernel.Kernel.RunAsync(plan);
            AnsiConsole.MarkupLine($"[{Skin.AgentStyle}]{Markup.Escape(result.GetValue<string>() ?? "")}[/]");
        });
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
        } else
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
