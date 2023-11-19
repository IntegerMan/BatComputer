using MattEland.BatComputer.Kernel;
using Spectre.Console;

namespace MattEland.BatComputer.ConsoleApp.Commands;

public class ShowMemoryCommand : AppCommand
{
    public ShowMemoryCommand(BatComputerApp app) : base(app)
    {
    }

    public override async Task ExecuteAsync(AppKernel kernel)
    {
        var collections = await App.Kernel!.Memory!.GetCollectionsAsync();

        if (!collections.Any())
        {
            AnsiConsole.MarkupLine($"[{Skin.WarningStyle}]No memory collections found.[/]");
        }
        else
        {
            AnsiConsole.MarkupLine($"[{Skin.NormalStyle}]Memory collections:[/]");
            foreach (string collection in collections)
            {
                AnsiConsole.MarkupLine($"[{Skin.AccentStyle}] - {collection}[/]");
            }
        }

        AnsiConsole.WriteLine();
    }

    public override string DisplayText => "Show Memory";

    public override bool CanExecute(AppKernel kernel) => kernel?.Memory != null;
}