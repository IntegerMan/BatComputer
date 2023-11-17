using MattEland.BatComputer.Kernel;
using Microsoft.Extensions.Configuration;
using Spectre.Console;
using System.Reflection;
using MattEland.BatComputer.Abstractions;
using MattEland.BatComputer.ConsoleApp.Abstractions;

namespace MattEland.BatComputer.ConsoleApp;

public static class SettingsLoader
{
    public static KernelSettings Load(this KernelSettings settings, ConsoleSkin skin)
    {
        // Load settings
        IConfigurationRoot config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddUserSecrets(Assembly.GetExecutingAssembly())
            .AddEnvironmentVariables()
            .AddCommandLine(Environment.GetCommandLineArgs())
            .Build();

        AnsiConsole.MarkupLine($"[{skin.NormalStyle}]Loading configuration[/]");
        ReadRequiredSetting(config, skin, "AzureAIEndpoint", v => settings.AzureAiServicesEndpoint = v);
        ReadRequiredSetting(config, skin, "AzureAIKey", v => settings.AzureAiServicesKey = v);
        ReadRequiredSetting(config, skin, "AzureOpenAIEndpoint", v => settings.AzureOpenAiEndpoint = v);
        ReadRequiredSetting(config, skin, "AzureOpenAIKey", v => settings.AzureOpenAiKey = v);
        ReadRequiredSetting(config, skin, "OpenAIDeploymentName", v => settings.OpenAiDeploymentName = v);
        ReadOptionalSetting(config, skin, "BingKey", v => settings.BingKey = v);
        ReadOptionalSetting(config, skin, "SessionizeToken", v => settings.SessionizeToken = v);
        AnsiConsole.WriteLine();

        return settings;
    }

    private static void ReadRequiredSetting(IConfiguration config, ConsoleSkin skin, string settingName, Action<string> applyAction)
    {
        string? value = config[settingName];
        if (!string.IsNullOrEmpty(value))
        {
            applyAction(value);
        }
        else
        {
            AnsiConsole.MarkupLine($"[{skin.ErrorStyle}]{skin.ErrorEmoji} Could not read the config value for {settingName}[/]");
        }
    }
    private static void ReadOptionalSetting(IConfiguration config, ConsoleSkin skin, string settingName, Action<string> applyAction)
    {
        string? value = config[settingName];
        if (!string.IsNullOrEmpty(value))
        {
            applyAction(value);
        }
    }
}
