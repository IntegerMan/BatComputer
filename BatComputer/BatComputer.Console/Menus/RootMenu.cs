using MattEland.BatComputer.Abstractions;
using MattEland.BatComputer.ConsoleApp.Commands;

namespace MattEland.BatComputer.ConsoleApp.Menus;

public class RootMenu : MenuBase
{
    public RootMenu(BatComputerApp app) : base(app)
    {
    }

    public override IEnumerable<AppCommand> Commands
    {
        get
        {
            yield return new SemanticQueryCommand(App);
            yield return new ChatCommand(App);
            yield return new RetryCommand(App);
            yield return new ListPluginsCommand(App);
            yield return new ShowPlanTreeCommand(App);
            yield return new ShowPlanJsonCommand(App);

            // TODO: These probably belong in a submenu
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

            yield return new ExitCommand(App, title: "Quit", confirm: true);
        }
    }
}
