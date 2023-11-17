using MattEland.BatComputer.Kernel;
using Microsoft.Extensions.Configuration;
using Spectre.Console;
using System.Reflection;
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

        // TODO: I shouldn't need to manually map these. Deserialize as needed using standard .NET code

        AnsiConsole.MarkupLine($"[{skin.NormalStyle}]Loading configuration[/]");
        ReadRequiredSetting(config, skin, "AzureAIEndpoint", v => settings.AzureAiServicesEndpoint = v);
        ReadRequiredSetting(config, skin, "AzureAIKey", v => settings.AzureAiServicesKey = v);
        ReadRequiredSetting(config, skin, "AzureAIRegion", v => settings.AzureAiServicesRegion = v);
        ReadRequiredSetting(config, skin, "AzureOpenAIEndpoint", v => settings.AzureOpenAiEndpoint = v);
        ReadRequiredSetting(config, skin, "AzureOpenAIKey", v => settings.AzureOpenAiKey = v);
        ReadRequiredSetting(config, skin, "OpenAIDeploymentName", v => settings.OpenAiDeploymentName = v);
        ReadOptionalSetting(config, skin, "BingKey", v => settings.BingKey = v);
        ReadOptionalSetting(config, skin, "SessionizeToken", v => settings.SessionizeToken = v);
        ReadOptionalSetting(config, skin, "SpeechVoiceName", v => settings.SpeechVoiceName = v!, "en-GB-AlfieNeural");
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
    private static void ReadOptionalSetting(IConfiguration config, ConsoleSkin skin, string settingName, Action<string?> applyAction, string? defaultValue = null)
    {
        string? value = config[settingName];
        applyAction(string.IsNullOrEmpty(value) ? defaultValue : value);
    }
}
