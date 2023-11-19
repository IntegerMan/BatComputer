using MattEland.BatComputer.ConsoleApp.Helpers;
using MattEland.BatComputer.Kernel;
using Spectre.Console;

namespace MattEland.BatComputer.ConsoleApp.Commands;

public class AddMemoryCommand : AppCommand
{
    public AddMemoryCommand(BatComputerApp app) : base(app)
    {
    }

    public override async Task ExecuteAsync(AppKernel kernel)
    {
        try
        {
            string id = InputHelpers.GetUserText($"[{Skin.NormalStyle}]Memory Id:[/]");
            string information = InputHelpers.GetUserText($"[{Skin.NormalStyle}]Contents:[/]");
            string description = InputHelpers.GetUserText($"[{Skin.NormalStyle}]Description:[/]");

            string result = "";

            await AnsiConsole.Status().StartAsync($"Saving memory...", async ctx =>
            {
                ctx.Spinner = Skin.Spinner;
                result = await App.Kernel!.Memory!.SaveInformationAsync(kernel.MemoryCollectionName!, information, id, description);
            });

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[{Skin.SuccessStyle}]Added memory[/] [{Skin.AccentStyle}]{Markup.Escape(result)}[/]");
            AnsiConsole.WriteLine();
        }
        catch (Exception ex)
        {
            OutputHelpers.WriteException(Skin, ex);
        }
    }

    public override string DisplayText => "Add Memory";

    public override bool CanExecute(AppKernel kernel) => kernel?.Memory != null && kernel.MemoryCollectionName != null!;
}