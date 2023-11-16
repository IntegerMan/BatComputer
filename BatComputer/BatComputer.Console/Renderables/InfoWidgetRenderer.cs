using MattEland.BatComputer.Abstractions.Widgets;
using MattEland.BatComputer.ConsoleApp.Abstractions;
using Spectre.Console;

namespace MattEland.BatComputer.ConsoleApp.Renderables;

// ReSharper disable once UnusedMember.Global - Used via Reflection
public class InfoWidgetRenderer : WidgetRenderer<InfoWidget>
{
    public override void Render(InfoWidget widget, ConsoleSkin skin)
    {
        Table table = new Table().AddColumns($"[{skin.NormalStyle}]{Markup.Escape(widget.Title)}[/]")
            .Centered()
            .Border(TableBorder.Rounded)
            .BorderStyle(skin.AccentStyle)
            .AddRow($"[{skin.DebugStyle}]{Markup.Escape(widget.Message)}[/]");

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
    }
}
