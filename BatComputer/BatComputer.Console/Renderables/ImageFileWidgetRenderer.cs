using MattEland.BatComputer.Abstractions.Widgets;
using MattEland.BatComputer.ConsoleApp.Abstractions;
using MattEland.BatComputer.ConsoleApp.Helpers;
using Spectre.Console;

namespace MattEland.BatComputer.ConsoleApp.Renderables;

// ReSharper disable once UnusedMember.Global - Used via Reflection
public class ImageFileWidgetRenderer : WidgetRenderer<ImageFileWidget>
{
    public override void Render(ImageFileWidget widget, ConsoleSkin skin)
    {
        OutputHelpers.RenderImage(widget.Path, maxWidth: 30);
        AnsiConsole.WriteLine();
    }
}
