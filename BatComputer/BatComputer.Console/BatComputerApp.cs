using Microsoft.Extensions.Configuration;
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

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[Yellow]Program complete[/]");

        return 0;
    }

    private Action<StatusContext> LoadSettings(string[] args)
    {
        return ctx => {
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
