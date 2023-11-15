using System.Reflection;
using BatComputer.Abstractions;
using BatComputer.Skins;
using MattEland.BatComputer.Abstractions;
using MattEland.BatComputer.ConsoleApp.Commands;
using MattEland.BatComputer.ConsoleApp.Menus;
using MattEland.BatComputer.Kernel;
using MattEland.BatComputer.ConsoleApp.Renderables;
using Spectre.Console;

namespace MattEland.BatComputer.ConsoleApp;

public class BatComputerApp
{
    private readonly KernelSettings _settings = new();
    public ConsoleSkin Skin { get; set; } = new BatComputerSkin();

    public async Task<int> RunAsync()
    {
        WelcomeRenderer.ShowWelcome(Skin);

        _settings.Load(Skin);
        _settings.Validate();

        AppKernel appKernel = new(_settings);
        appKernel.RenderKernelPluginsChart(Skin);

        Menus.Push(new RootMenu(this));

        await RunMainLoopAsync(appKernel);

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[{Skin.SuccessStyle}]Program complete[/]");

        return 0;
    }

    public Stack<MenuBase> Menus { get; } = new();

    private async Task RunMainLoopAsync(AppKernel appKernel)
    {
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
