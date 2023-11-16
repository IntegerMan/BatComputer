using MattEland.BatComputer.Abstractions;
using MattEland.BatComputer.ConsoleApp.Abstractions;

namespace MattEland.BatComputer.ConsoleApp.Renderables;

public abstract class WidgetRenderer<T> where T : IWidget
{
    public abstract void Render(T widget, ConsoleSkin skin);
}