using MattEland.BatComputer.Abstractions.Widgets;
using MattEland.BatComputer.ConsoleApp.Abstractions;
using Spectre.Console;

namespace MattEland.BatComputer.ConsoleApp.Renderables;

// ReSharper disable once UnusedMember.Global - Used via Reflection
public class InfoWidgetRenderer : WidgetRenderer<InfoWidget>
{
    public override void Render(InfoWidget widget, ConsoleSkin skin)
    {
        Panel box = new Panel($"[{skin.DebugStyle}]{Markup.Escape(widget.Message)}[/]")
            .BorderStyle(skin.AccentStyle)
            .Header($"[{skin.NormalStyle}]{Markup.Escape(widget.Title)}[/]", Justify.Left);

        AnsiConsole.Write(box);
        AnsiConsole.WriteLine();
    }
}
