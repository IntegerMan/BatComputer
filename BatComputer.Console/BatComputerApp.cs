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
        OutputHelpers.DisplayPendingWidgets(this);
        AnsiConsole.WriteLine();
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
    public Queue<IWidget> Widgets { get; } = new();

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

    public void Dispose()
    {
        Speech?.Dispose();
    }

    public void ReportTokenUsage(int promptTokens, int completionTokens)
    {
        _tokenUsage.Add(new TokenUsage(promptTokens, TokenUsageType.Prompt));
        _tokenUsage.Add(new TokenUsage(completionTokens, TokenUsageType.Completion));
    }

    public void RenderTokenUsage()
    {
        if (_tokenUsage.Count > 0)
        {
            TokenUsageWidget widget = new(_tokenUsage);
            widget.Render(Skin);

            _tokenUsage.Clear();
        }
    }

    private readonly List<TokenUsage> _tokenUsage = new();

}
