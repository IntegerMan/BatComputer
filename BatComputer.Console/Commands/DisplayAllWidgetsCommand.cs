using MattEland.BatComputer.Abstractions.Widgets;
using MattEland.BatComputer.ConsoleApp.Helpers;

namespace MattEland.BatComputer.ConsoleApp.Commands;

public class DisplayAllWidgetsCommand : AppCommand
{
    public override Task ExecuteAsync()
    {
        IEnumerable<Type> widgetTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => typeof(IWidget).IsAssignableFrom(type) && !type.IsAbstract);

        foreach (Type widgetType in widgetTypes)
        {
            IWidget widget = (IWidget)Activator.CreateInstance(widgetType)!;

            widget.UseSampleData();
            widget.Render(Skin);
        }

        return Task.CompletedTask;
    }

    public override string DisplayText => "Display all sample widgets";

    public DisplayAllWidgetsCommand(BatComputerApp app) : base(app)
    {
    }
}
