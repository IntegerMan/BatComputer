using BatComputer.Abstractions;
using BatComputer.Skins;
using MattEland.BatComputer.ConsoleApp.Commands;
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

        LoadSettings();

        AppKernel appKernel = new(_settings);
        appKernel.RenderKernelPluginsChart(Skin);

        await RunMainLoopAsync(appKernel);

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[{Skin.SuccessStyle}]Program complete[/]");

        return 0;
    }

    private async Task RunMainLoopAsync(AppKernel appKernel)
    {
        do
        {
            SelectionPrompt<AppCommand> choices = new SelectionPrompt<AppCommand>()
                    .Title($"[{Skin.NormalStyle}]Select an action[/]")
                    .HighlightStyle(Skin.AccentStyle)
                    .AddChoices(GetMainMenuOptions().Where(c => c.CanExecute(appKernel)))
                    .UseConverter(c => c.DisplayText);

            AppCommand choice = AnsiConsole.Prompt(choices);
            await choice.ExecuteAsync( appKernel);
        } while (!ExitRequested);
    }

    public bool ExitRequested { get; set; }

    private IEnumerable<AppCommand> GetMainMenuOptions()
    {
        yield return new SemanticQueryCommand(this);
        yield return new ChatCommand(this);
        yield return new RetryCommand(this);
        yield return new ListPluginsCommand(this);
        yield return new ShowPlanTreeCommand(this);
        yield return new ShowPlanJsonCommand(this);
        yield return new QuitCommand(this);
    }

    private void LoadSettings()
    {
        AnsiConsole.Status().Start("Loading Configuration", ctx =>
        {
            ctx.Spinner(Skin.Spinner);

            _settings.Load(Skin);

            ctx.Status("Validating settings");
            AnsiConsole.WriteLine();

            _settings.Validate();
        });
    }
}
