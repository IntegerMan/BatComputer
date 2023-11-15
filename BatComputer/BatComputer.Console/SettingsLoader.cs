﻿using MattEland.BatComputer.Kernel;
using Microsoft.Extensions.Configuration;
using Spectre.Console;
using System.Reflection;
using MattEland.BatComputer.Abstractions;
using MattEland.BatComputer.ConsoleApp.Abstractions;

namespace MattEland.BatComputer.ConsoleApp;

public static class SettingsLoader
{
    public static KernelSettings Load(this KernelSettings _settings, ConsoleSkin skin)
    {
        // Load settings
        IConfigurationRoot config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddUserSecrets(Assembly.GetExecutingAssembly())
            .AddEnvironmentVariables()
            .AddCommandLine(Environment.GetCommandLineArgs())
            .Build();

        AnsiConsole.MarkupLine($"[{skin.NormalStyle}]Reading configuration data[/]");
        ReadRequiredSetting(config, skin, "AzureAIEndpoint", v => _settings.AzureAiServicesEndpoint = v);
        ReadRequiredSetting(config, skin, "AzureAIKey", v => _settings.AzureAiServicesKey = v);
        ReadRequiredSetting(config, skin, "AzureOpenAIEndpoint", v => _settings.AzureOpenAiEndpoint = v);
        ReadRequiredSetting(config, skin, "AzureOpenAIKey", v => _settings.AzureOpenAiKey = v);
        ReadRequiredSetting(config, skin, "OpenAIDeploymentName", v => _settings.OpenAiDeploymentName = v);
        ReadOptionalSetting(config, skin, "BingKey", v => _settings.BingKey = v);

        AnsiConsole.WriteLine();

        return _settings;
    }

    private static void ReadRequiredSetting(IConfiguration config, ConsoleSkin skin, string settingName, Action<string> applyAction)
    {
        string? value = config[settingName];
        if (string.IsNullOrEmpty(value))
        {
            AnsiConsole.MarkupLine($"[{skin.ErrorStyle}]{skin.ErrorEmoji} Could not read the config value for {settingName}[/]");
        }
        else
        {
            applyAction(value);
            AnsiConsole.MarkupLine($"[{skin.SuccessStyle}]{skin.SuccessEmoji} Read setting {settingName}[/]");
        }
    }
    private static void ReadOptionalSetting(IConfiguration config, ConsoleSkin skin, string settingName, Action<string> applyAction)
    {
        string? value = config[settingName];
        if (string.IsNullOrEmpty(value))
        {
            AnsiConsole.MarkupLine($"[{skin.WarningStyle}]{skin.WarningEmoji} Could not read the config value for {settingName}[/]");
        }
        else
        {
            applyAction(value);
            AnsiConsole.MarkupLine($"[{skin.SuccessStyle}]{skin.SuccessEmoji} Read setting {settingName}[/]");
        }
    }
}
