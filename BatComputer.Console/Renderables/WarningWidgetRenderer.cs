using MattEland.BatComputer.Abstractions.Widgets;
using MattEland.BatComputer.ConsoleApp.Abstractions;
using Spectre.Console;

namespace MattEland.BatComputer.ConsoleApp.Renderables;

// ReSharper disable once UnusedMember.Global - Used via Reflection
public class WarningWidgetRenderer : WidgetRenderer<WarningWidget>
{
    public override void Render(WarningWidget widget, ConsoleSkin skin)
    {
        Panel box = new Panel($"[{skin.NormalStyle}]{Markup.Escape(widget.Message)}[/]")
            .Border(BoxBorder.Heavy)
            .BorderStyle(skin.WarningStyle)
            .Padding(2, 1)
            .Header($"[{skin.ErrorStyle}] {Markup.Escape(widget.Title.ToUpper())} [/]", Justify.Center);

        AnsiConsole.Write(box);
        AnsiConsole.WriteLine();
    }
}
