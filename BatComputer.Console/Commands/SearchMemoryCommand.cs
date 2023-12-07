using MattEland.BatComputer.Abstractions.Widgets;
using MattEland.BatComputer.ConsoleApp.Helpers;
using MattEland.BatComputer.Kernel;
using Microsoft.SemanticKernel.Memory;
using Spectre.Console;

namespace MattEland.BatComputer.ConsoleApp.Commands;

public class SearchMemoryCommand : AppCommand
{
    public SearchMemoryCommand(BatComputerApp app, string collectionName) : base(app)
    {
        CollectionName = collectionName;
    }

    public override async Task ExecuteAsync()
    {
        try
        {
            string collection = CollectionName;
            string query = InputHelpers.GetUserText($"[{Skin.NormalStyle}]Query:[/]");

            List<MemoryQueryResult> results = new();
            await AnsiConsole.Status().StartAsync($"Searching memory…", async ctx =>
            {
                ctx.Spinner = Skin.Spinner;
                IAsyncEnumerable<MemoryQueryResult>? asyncResults = AsyncEnumerable.Empty<MemoryQueryResult>();// App.Kernel!.Memory!.SearchAsync(collection, query, limit: 10);

                await foreach (MemoryQueryResult result in asyncResults)
                {
                    results.Add(result);
                }
            });

            if (results.Count == 0)
            {
                AnsiConsole.MarkupLine($"[{Skin.WarningStyle}]No results found[/]");
            }
            else
            {
                foreach (MemoryQueryResult result in results)
                {
                    OutputHelpers.Render(new MemoryQueryResultWidget(collection, result), Skin);
                }
            }

            AnsiConsole.WriteLine();
        }
        catch (Exception ex)
        {
            OutputHelpers.WriteException(Skin, ex);
        }
    }

    public override string DisplayText => "Search Memory";

    public string CollectionName { get; }

    public override bool CanExecute() => false;// kernel?.Memory != null && kernel.MemoryCollectionName != null!;
}