using Dumpify;
using MattEland.BatComputer.Abstractions.Widgets;
using MattEland.BatComputer.ConsoleApp.Abstractions;

namespace MattEland.BatComputer.ConsoleApp.Renderables;

public class DataWidgetRenderer : WidgetRenderer<DataWidget>
{
    public override void Render(DataWidget widget, ConsoleSkin skin)
    {
        widget.Data.Dump(label: widget.Title);
    }
}
