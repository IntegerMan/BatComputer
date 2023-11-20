using DocumentFormat.OpenXml.Office2010.Excel;
using Google.Apis.CustomSearchAPI.v1.Data;
using MattEland.BatComputer.Kernel;
using Microsoft.SemanticKernel.Memory;
using Microsoft.VisualBasic;
using Spectre.Console;

namespace MattEland.BatComputer.ConsoleApp.Commands;

public class ShowMemoryCommand : AppCommand
{
    public ShowMemoryCommand(BatComputerApp app) : base(app)
    {
    }

    public override async Task ExecuteAsync(AppKernel kernel)
    {
        ISemanticTextMemory memory = App.Kernel!.Memory!;

        IList<string> collections = new List<string>(0);
        await AnsiConsole.Status().StartAsync($"Getting collections…", async ctx =>
        {
            ctx.Spinner = Skin.Spinner;
            collections = await memory.GetCollectionsAsync();
        });         

        if (!collections.Any())
        {
            AnsiConsole.MarkupLine($"[{Skin.WarningStyle}]No memory collections found.[/]");
        }
        else
        {
            AnsiConsole.MarkupLine($"[{Skin.NormalStyle}]Memory collections:[/]");
            foreach (string collection in collections)
            {
                AnsiConsole.MarkupLine($"[{Skin.DebugStyle}] - {Markup.Escape(collection)}[/]");

                /* TODO: I'd love to be able to list keys in the collection
                IAsyncEnumerable<MemoryQueryResult> results = memory.SearchAsync(collection, string.Empty, limit: 100, minRelevanceScore: 0);
                await foreach (MemoryQueryResult result in results)
                {
                    AnsiConsole.MarkupLine($"[{Skin.NormalStyle}]     - {Markup.Escape(result.ToString())}[/]");

                }
                */
            }
        }

        AnsiConsole.WriteLine();
    }

    public override string DisplayText => "Show Memories";

    public override bool CanExecute(AppKernel kernel) => kernel?.Memory != null;
}