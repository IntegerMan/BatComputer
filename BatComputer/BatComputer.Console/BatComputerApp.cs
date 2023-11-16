using MattEland.BatComputer.Abstractions.Widgets;
using MattEland.BatComputer.ConsoleApp.Abstractions;
using MattEland.BatComputer.ConsoleApp.Commands;
using MattEland.BatComputer.ConsoleApp.Helpers;
using MattEland.BatComputer.ConsoleApp.Menus;
using MattEland.BatComputer.Kernel;
using MattEland.BatComputer.ConsoleApp.Renderables;
using MattEland.BatComputer.ConsoleApp.Skins;
using Spectre.Console;

namespace MattEland.BatComputer.ConsoleApp;

public class BatComputerApp
{
    private readonly KernelSettings _settings = new();
    public ConsoleSkin Skin { get; set; } = new BatComputerSkin();

    public async Task<int> RunAsync()
    {
        // Fancy header
        WelcomeRenderer.ShowWelcome(Skin);

        // Handle app settings
        _settings.Load(Skin);
        _settings.Validate();
        if (!_settings.SupportsSearch)
        {
            new WarningWidget("No Bing Search Key Supplied. Web Search will be disabled.").Render(Skin);
        }

        // Load plugins and display loaded plugins
        AppKernel appKernel = new(_settings);

        // Warn the user that actual costs may be incurred from using the app
        new InfoWidget("Disclaimer","This app uses chat, text completions, vision, speech, search, and other features that have costs associated with them. The developer will not be responsible for costs resulting from its usage.").Render(Skin);
        if (AnsiConsole.Confirm("Do you agree to these terms and wish to continue?"))
        {
            // Show plugins now that they're paying attention
            AnsiConsole.WriteLine();
            appKernel.RenderKernelPluginsChart(Skin);
            AnsiConsole.WriteLine();

            // Primary loop
            AnsiConsole.MarkupLine($"[{Skin.SuccessStyle}]System Ready[/]");
            Menus.Push(new RootMenu(this));
            await RunMainLoopAsync(appKernel);
        }

        // Indicate success on exit (for a fun time, ask me why I always log when a program completes)
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[{Skin.SuccessStyle}]Program complete[/]");
        return 0;
    }

    public Stack<MenuBase> Menus { get; } = new();

    private async Task RunMainLoopAsync(AppKernel appKernel)
    {
        AnsiConsole.WriteLine();

        while (Menus.TryPeek(out MenuBase? activeMenu))
        {
            SelectionPrompt<AppCommand> choices = new SelectionPrompt<AppCommand>()
                    .Title($"[{Skin.NormalStyle}]Select an action[/]")
                    .HighlightStyle(Skin.AccentStyle)
                    .AddChoices(activeMenu.Commands.Where(c => c.CanExecute(appKernel)))
                    .UseConverter(c => c.DisplayText);

            AppCommand choice = AnsiConsole.Prompt(choices);

            await choice.ExecuteAsync( appKernel);
        }
    }
}
