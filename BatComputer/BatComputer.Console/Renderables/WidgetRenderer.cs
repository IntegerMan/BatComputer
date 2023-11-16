using MattEland.BatComputer.Abstractions;
using MattEland.BatComputer.Abstractions.Widgets;
using MattEland.BatComputer.ConsoleApp.Abstractions;

namespace MattEland.BatComputer.ConsoleApp.Renderables;

public abstract class WidgetRenderer<T> where T : IWidget, new()
{
    public abstract void Render(T widget, ConsoleSkin skin);
}