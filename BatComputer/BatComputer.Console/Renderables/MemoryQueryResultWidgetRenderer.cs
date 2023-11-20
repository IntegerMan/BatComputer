using MattEland.BatComputer.Abstractions.Widgets;
using MattEland.BatComputer.ConsoleApp.Abstractions;
using Spectre.Console;

namespace MattEland.BatComputer.ConsoleApp.Renderables;

public class MemoryQueryResultWidgetRenderer : WidgetRenderer<MemoryQueryResultWidget>
{
    public override void Render(MemoryQueryResultWidget widget, ConsoleSkin skin)
    {
        Table table = new Table()
                    .AddColumns("", "")
                    .HideHeaders()
                    .Title(widget.Title ?? "Memory Query Results", style: skin.AccentStyle)
                    .AddRow($"[{skin.NormalStyle}]Value:[/]", widget.Text ?? "None")
                    .AddRow($"[{skin.NormalStyle}]Description:[/]", widget.Description ?? "None")
                    .Centered();

        if (!string.IsNullOrWhiteSpace(widget.ExternalSourceName))
        {
            table.AddRow($"[{skin.NormalStyle}]Source:[/]", widget.ExternalSourceName);
        }

        BarChart bar = new BarChart().AddItem(widget.Relevance.ToString("P"), widget.Relevance, skin.ChartColor1).WithMaxValue(1.0).HideValues().RightAlignLabel();

        table.AddRow(new Markup($"[{skin.NormalStyle}]Relevance:[/]"), bar);

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
    }
}
