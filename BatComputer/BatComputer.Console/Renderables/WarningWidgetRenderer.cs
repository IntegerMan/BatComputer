using MattEland.BatComputer.Abstractions.Widgets;
using MattEland.BatComputer.ConsoleApp.Abstractions;
using Spectre.Console;

namespace MattEland.BatComputer.ConsoleApp.Renderables;

// ReSharper disable once UnusedMember.Global - Used via Reflection
public class WarningWidgetRenderer : WidgetRenderer<WarningWidget>
{
    public override void Render(WarningWidget widget, ConsoleSkin skin)
    {
        Table table = new Table().AddColumns($"[{skin.WarningStyle}]{Markup.Escape(widget.Title)}[/]")
            .Centered()
            .Border(TableBorder.HeavyEdge)
            .BorderStyle(skin.WarningStyle)
            .AddRow($"[{skin.NormalStyle}]{Markup.Escape(widget.Message)}[/]");

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
    }
}
