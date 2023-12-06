using MattEland.BatComputer.Abstractions.Widgets;

namespace MattEland.BatComputer.Abstractions;

public interface IAppKernel
{
    Queue<IWidget> Widgets { get; }
    void AddWidget(IWidget widget);
}