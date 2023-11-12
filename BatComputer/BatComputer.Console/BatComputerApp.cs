using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MattEland.BatComputer.ConsoleApp;

public class BatComputerApp
{
    private const string SystemText = "You are an AI assistant named Alfred. Respond to the user as if the user was Batman";
    private readonly BatComputerSettings _settings = new();
    public ConsoleSkin Skin { get; set; } = new BatComputerSkin();

    public int Run(string[] args)
    {
        // Stylize the app
        Color colorPrimary = Skin.NormalColor;
        Style primary = new(foreground: colorPrimary);
        AnsiConsole.Foreground = colorPrimary;

        // Show the main welcome header
        ShowWelcomeMenu(colorPrimary, primary);

        AnsiConsole.Status().Start("Loading Configuration", LoadSettings(args));

        string userText = AnsiConsole.Ask<string>($"[{Skin.NormalStyle}]Enter a question for OpenAI:[/]");
        AnsiConsole.WriteLine();

        KernelBuilder builder = new KernelBuilder();
        builder.WithAzureOpenAIChatCompletionService("gpt35turbo", _settings.AzureOpenAiEndpoint, _settings.AzureOpenAiKey);
        IKernel kernel = builder.Build();

        OpenAIClient aiClient = new(new Uri(_settings.AzureOpenAiEndpoint), new AzureKeyCredential(_settings.AzureOpenAiKey));

        AnsiConsole.Status().Start("Waiting for response", ctx =>
        {
            ctx.Spinner(Skin.Spinner);

            /*
            ChatCompletionsOptions chatOptions = new()
            {
                ChoiceCount = 1,
                DeploymentName = "gpt35turbo", // TODO: Take from settings
                User = Guid.NewGuid().ToString(), // TODO: Machine name
            };
            chatOptions.Messages.Add(new ChatMessage(ChatRole.System, SystemText));
            chatOptions.Messages.Add(new ChatMessage(ChatRole.User, userText));
            */

            ISKFunction func = kernel.CreateSemanticFunction(SystemText, new Microsoft.SemanticKernel.Connectors.AI.OpenAI.OpenAIRequestSettings());
            Microsoft.SemanticKernel.Orchestration.KernelResult result = kernel.RunAsync(userText, func).Result;

            //Response<ChatCompletions> result = aiClient.GetChatCompletions(chatOptions);

            //AnsiConsole.MarkupLine($"[{Skin.AgentStyle}]{result.Value.Choices.First().Message.Content}[/]"); // TODO: Escape markup
            AnsiConsole.MarkupLine($"[{Skin.AgentStyle}]{result}[/]"); // TODO: Escape markup
        });

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[{Skin.SuccessStyle}]Program complete[/]");

        return 0;
    }

    private Action<StatusContext> LoadSettings(string[] args)
    {
        return ctx =>
        {
            ctx.Spinner(Skin.Spinner);

            // Load settings
            IConfigurationRoot config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddUserSecrets(Assembly.GetExecutingAssembly())
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build();

            AnsiConsole.MarkupLine("[Yellow]Reading configuration data[/]");
            ReadRequiredSetting(config, "AzureAIEndpoint", v => _settings.AzureAiServicesEndpoint = v);
            ReadRequiredSetting(config, "AzureAIKey", v => _settings.AzureAiServicesKey = v);
            ReadRequiredSetting(config, "AzureOpenAIEndpoint", v => _settings.AzureOpenAiEndpoint = v);
            ReadRequiredSetting(config, "AzureOpenAIKey", v => _settings.AzureOpenAiKey = v);

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
        };
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

        Version version = Assembly.GetExecutingAssembly().GetName().Version!;

        AnsiConsole.Write(new Text($"Version {version}", style: primary).RightJustified());
        AnsiConsole.WriteLine();

        AnsiConsole.WriteLine();
    }
}
