using MattEland.BatComputer.Abstractions;
using MattEland.BatComputer.ConsoleApp.Helpers;
using MattEland.BatComputer.Kernel;

namespace MattEland.BatComputer.ConsoleApp.Commands;

public class DisplaySampleWidgetCommand : AppCommand
{
    private readonly Func<IWidget> _widgetFactory;
    private readonly string _widgetName;

    public override Task ExecuteAsync(AppKernel kernel)
    {
        IWidget widget = _widgetFactory();
        widget.UseSampleData();
        widget.Render();

        return Task.CompletedTask;
    }

    public override string DisplayText => $"Display Test {_widgetName}";

    public DisplaySampleWidgetCommand(BatComputerApp app, Func<IWidget> widgetFactory, string widgetName) : base(app)
    {
        _widgetFactory = widgetFactory;
        _widgetName = widgetName;
    }
}
