﻿using MattEland.BatComputer.Memory.FileMemoryStore;
using Microsoft.SemanticKernel.Memory;
using Spectre.Console;

namespace MattEland.BatComputer.ConsoleApp.Commands;

public class ShowMemoryCommand : AppCommand
{
    private readonly ISemanticTextMemory _memory;

    public ShowMemoryCommand(BatComputerApp app, ISemanticTextMemory memory) : base(app)
    {
        _memory = memory;
    }

    public override async Task ExecuteAsync()
    {
        // Normally you can't get at the keys in memory easily, but I've exposed them for demo / debugging purposes in FileBackedMemory
        FileBackedMemory? fileMem = _memory as FileBackedMemory;
        if (fileMem != null)
        {
            DisplayMemoryStore(fileMem);
        }
        else
        {
            // Fallback mode queries and shows the collection names only
            await DisplayMemoryAsync();
        }

        AnsiConsole.WriteLine();
    }

    private void DisplayMemoryStore(FileBackedMemory fileMem)
    {
        Tree memTree = new($"[{Skin.SuccessStyle}]Memory[/]");

        foreach (MemoryRecordCollection collection in fileMem)
        {
            TreeNode collectionTree = memTree.AddNode($"[{Skin.NormalStyle}]{Markup.Escape(collection.Collection)}[/]");

            foreach (MemoryRecord record in collection.Records.OrderBy(r => r.Metadata.Id))
            {
                collectionTree.AddNode($"[{Skin.AccentStyle}]{Markup.Escape(record.Key)}[/]: [{Skin.DebugStyle}]{Markup.Escape(record.Metadata.Description)}[/]");
            }
        }
        AnsiConsole.Write(memTree);
    }

    private async Task DisplayMemoryAsync()
    {
        IList<string> collections = new List<string>(0);
        await AnsiConsole.Status().StartAsync($"Getting collections…", async ctx =>
        {
            ctx.Spinner = Skin.Spinner;
            collections = await _memory.GetCollectionsAsync();
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
            }
        }
    }

    public override string DisplayText => "Show Memories";
}