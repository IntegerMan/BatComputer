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
        AnsiConsole.Write(new TextPath(widget.Path)
            .SeparatorStyle(skin.NormalStyle)
            .LeafStyle(skin.AccentStyle)
            .StemStyle(skin.DebugStyle)
            .RootStyle(skin.DebugStyle));
        AnsiConsole.WriteLine();
    }
}
