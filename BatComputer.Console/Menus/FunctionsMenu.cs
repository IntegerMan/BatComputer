using MattEland.BatComputer.ConsoleApp.Commands;

namespace MattEland.BatComputer.ConsoleApp.Menus;

public class FunctionsMenu : MenuBase
{
    public FunctionsMenu(BatComputerApp app) : base(app)
    {
    }

    public override IEnumerable<AppCommand> Commands
    {
        get
        {
            yield return new ListPluginsCommand(App);

            foreach (var plugin in Kernel.Functions.GetFunctionViews()
                .Where(f => !App.IsFunctionExcluded(f))
                .GroupBy(f => f.PluginName)
                .OrderBy(f => f.Key))
            {

                yield return new SubmenuCommand(App, plugin.Key, new PluginFunctionListMenu(App, plugin.Key));
            }

            yield return new ExitCommand(App, title: "Back");
        }
    }
}
