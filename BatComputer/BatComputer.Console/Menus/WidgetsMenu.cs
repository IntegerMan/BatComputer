using MattEland.BatComputer.Abstractions.Widgets;
using MattEland.BatComputer.ConsoleApp.Commands;

namespace MattEland.BatComputer.ConsoleApp.Menus;

public class WidgetsMenu : MenuBase
{
    public WidgetsMenu(BatComputerApp app) : base(app)
    {
    }

    public override IEnumerable<AppCommand> Commands
    {
        get
        {
            yield return new DisplayAllWidgetsCommand(App);

            // Add Diagnostics for each IWidget 
            IEnumerable<Type> widgetTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => typeof(IWidget).IsAssignableFrom(type) && !type.IsAbstract);

            foreach (Type widgetType in widgetTypes)
            {
                yield return new DisplaySampleWidgetCommand(App,
                    () => (IWidget)Activator.CreateInstance(widgetType)!,
                    widgetType.Name);
            }

            yield return new ExitCommand(App, title: "Back");
        }
    }
}
