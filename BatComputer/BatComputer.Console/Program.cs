using Spectre.Console;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace MattEland.BatComputer.ConsoleApp;

internal class Program {
    private static int Main(string[] args) {

        try {
            // Using UTF8 allows more capabilities for Spectre Console.
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;

            // Stylize the app
            ConsoleSkin skin = new BatComputerSkin();
            Color colorPrimary = Color.Yellow;
            Style primary = new(foreground: colorPrimary);
            AnsiConsole.Foreground = colorPrimary;

            // Show the main welcome header
            ShowWelcomeMenu(colorPrimary, primary, skin);

            BatComputerSettings settings = new();
            AnsiConsole.Status().Start("Loading Configuration", LoadSettings(args, skin, settings));

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[Yellow]Program complete[/]");

            return 0;
        }
        catch (Exception ex) {
            AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);
            return -1;
        }
    }

    private static Action<StatusContext> LoadSettings(string[] args, ConsoleSkin skin, BatComputerSettings settings) {
        return ctx => {
            ctx.Spinner(skin.Spinner);

            // Load settings
            IConfigurationRoot config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddUserSecrets(Assembly.GetExecutingAssembly())
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build();

            AnsiConsole.MarkupLine("[Yellow]Reading configuration data[/]");
            ReadRequiredSetting(config, "AzureAIEndpoint", v => settings.AzureAiServicesEndpoint = v);
            ReadRequiredSetting(config, "AzureAIKey", v => settings.AzureAiServicesKey = v);
            ReadRequiredSetting(config, "AzureOpenAIEndpoint", v => settings.AzureOpenAiEndpoint = v);
            ReadRequiredSetting(config, "AzureOpenAIKey", v => settings.AzureOpenAiKey = v);

            ctx.Status("Validating settings");
            AnsiConsole.WriteLine();

            if (!settings.IsValid) {
                StringBuilder sb = new();
                sb.AppendLine("Settings were in an invalid state after all settings were parsed:");
                foreach (System.ComponentModel.DataAnnotations.ValidationResult violation in settings.Validate(new ValidationContext(settings))) {
                    sb.AppendLine($"- {violation.ErrorMessage}");
                }
                throw new InvalidOperationException(sb.ToString());
            }

            AnsiConsole.MarkupLine("[Yellow]Finished reading config settings[/]");
            AnsiConsole.WriteLine();
        };
    }

    private static void ReadRequiredSetting(IConfigurationRoot config, string settingName, Action<string> applyAction) {
        string? value = config[settingName];
        if (string.IsNullOrEmpty(value)) {
            AnsiConsole.MarkupLine($"[Red]:stop_sign: Could not read the config value for {settingName}[/]");
        } else {
            applyAction(value);
            AnsiConsole.MarkupLine($"[Green]:check_mark_button: Read setting {settingName}[/]");
        }
    }

    private static void ShowWelcomeMenu(Color colorPrimary, Style primary, ConsoleSkin skin) {
        AnsiConsole.WriteLine($"Welcome to {skin.AppNamePrefix}".TrimEnd());

        AnsiConsole.Write(new FigletText(FigletFont.Default, skin.AppName)
            .Centered()
            .Color(colorPrimary));

        Version version = Assembly.GetExecutingAssembly().GetName().Version!;

        AnsiConsole.Write(new Text($"Version {version}", style: primary).RightJustified());
        AnsiConsole.WriteLine();

        AnsiConsole.WriteLine();
    }
}