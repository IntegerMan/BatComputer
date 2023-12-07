using MattEland.BatComputer.Kernel;
using MattEland.BatComputer.Abstractions.Widgets;
using MattEland.BatComputer.Abstractions.Strategies;
using MattEland.BatComputer.ConsoleApp.Abstractions;
using MattEland.BatComputer.ConsoleApp.Commands;
using MattEland.BatComputer.ConsoleApp.Helpers;
using MattEland.BatComputer.ConsoleApp.Menus;
using MattEland.BatComputer.ConsoleApp.Renderables;
using MattEland.BatComputer.ConsoleApp.Skins;
using MattEland.BatComputer.Speech;
using MattEland.BatComputer.Abstractions;
using Spectre.Console;
using Microsoft.SemanticKernel.Diagnostics;

namespace MattEland.BatComputer.ConsoleApp;

public class BatComputerApp : IDisposable
{
    public ConsoleSkin Skin { get; set; } = new BatComputerSkin();
    public AppKernel? Kernel { get; private set; }

    public async Task<int> RunAsync()
    {
        // Fancy header
        WelcomeRenderer.ShowWelcome(Skin);

        // Handle app settings
        Settings.Load(Skin);
        Settings.Validate();
        if (!Settings.SupportsSearch)
        {
            new WarningWidget("No Bing Search Key Supplied. Web Search will be disabled.").Render(Skin);
        }
        if (!Settings.SupportsMemory)
        {
            new WarningWidget("No Embeddings Deployment Name or Embeddings Collection Name. Memory will be disabled.").Render(Skin);
        }

        // Warn the user that actual costs may be incurred from using the app
        if (!Settings.SkipCostDisclaimer)
        {
            new InfoWidget("Disclaimer", "This app uses chat, text completions, vision, speech, search, and other features that have costs associated with them. The developer will not be responsible for costs resulting from its usage.").Render(Skin);
            if (!AnsiConsole.Confirm("Do you agree to these terms and wish to continue?"))
            {
                AnsiConsole.MarkupLine($"[{Skin.SuccessStyle}]Program terminating[/]");
                return 0;
            }
        }

        // Greet the user and set up speech if it's configured
        if (Settings.SupportsAiServices)
        {
            Speech = new SpeechProvider(Settings.AzureAiServicesRegion, Settings.AzureAiServicesKey, Settings.SpeechVoiceName);
            Speech.EnableSpeech = Settings.IsSpeechEnabled;
            _ = Speech.SpeakAsync($"Welcome to {Skin.AppNameWithPrefix}");
        }

        // Configure the application
        PlannerStrategy planner = ChangePlannerCommand.SelectPlanner(Skin);
        Kernel = new(Settings, planner, new BatComputerLoggerFactory(this));

        // Show plugins now that they're paying attention
        Kernel.RenderKernelPluginsChart(Skin);
        AnsiConsole.WriteLine();

        // Primary loop
        AnsiConsole.MarkupLine($"[{Skin.SuccessStyle}]System Ready[/]");
        Menus.Push(new RootMenu(this));
        await RunMainLoopAsync();

        // Indicate success on exit (for a fun time, ask me why I always log when a program completes)
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[{Skin.SuccessStyle}]Program complete[/]");
        return 0;
    }

    public Stack<MenuBase> Menus { get; } = new();
    public KernelSettings Settings { get; } = new();
    public SpeechProvider? Speech { get; private set; }

    private async Task RunMainLoopAsync()
    {
        AnsiConsole.WriteLine();

        while (Menus.TryPeek(out MenuBase? activeMenu) && Kernel != null)
        {
            SelectionPrompt<AppCommand> choices = new SelectionPrompt<AppCommand>()
                    .Title($"[{Skin.NormalStyle}]Select an action[/]")
                    .HighlightStyle(Skin.AccentStyle)
                    .AddChoices(activeMenu.Commands.Where(c => c.CanExecute(Kernel)))
                    .UseConverter(c => c.DisplayText);

            AppCommand choice = AnsiConsole.Prompt(choices);

            await choice.ExecuteAsync(Kernel);
        }
    }

    public Task SpeakAsync(string message) => Speech?.SpeakAsync(message) ?? Task.CompletedTask;

    public void Dispose() => Speech?.Dispose();

    public void ReportTokenUsage(int promptTokens, int completionTokens)
    {
        TokenUsageWidget widget = new(new TokenUsage(promptTokens, TokenUsageType.Prompt), 
                                      new TokenUsage(completionTokens, TokenUsageType.Completion));

        widget.Render(Skin);
    }


    public async Task SendUserQueryAsync(string prompt)
    {
        if (Kernel == null)
        {
            throw new InvalidOperationException("Kernel must be initialized");
        }

        try
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[{Skin.NormalStyle}]Executing…[/]");
            AnsiConsole.WriteLine();
            await Kernel.ExecuteAsync(prompt);
        }
        catch (SKException ex)
        {
            // It's possible to reach the limits of what's possible with the planner. When that happens, handle it gracefully
            if (ex.Message.Contains("Not possible to create plan for goal", StringComparison.OrdinalIgnoreCase) ||
                ex.Message.Contains("Unable to create plan for goal with available functions", StringComparison.OrdinalIgnoreCase))
            {
                Skin.WriteErrorLine("It was not possible to fulfill this request with the available skills.");
            }
            else if (ex.Message.Contains("History is too long", StringComparison.OrdinalIgnoreCase))
            {
                Skin.WriteErrorLine("The planner caused too many tokens to be used to fulfill the request. There may be too many functions enabled.");
            }
            else if (ex.Message.Contains("Missing value for parameter"))
            {
                Skin.WriteErrorLine(ex.Message);
            }
            else
            {
                Skin.WriteException(ex);
            }
        }
        catch (InvalidCastException ex)
        {
            // Invalid Cast can happen with llamaSharp
            Skin.WriteException(ex);
            Skin.WriteErrorLine("Could not generate a plan.");
        }
        catch (Exception ex)
        {
            Skin.WriteException(ex);
        }
    }

}
