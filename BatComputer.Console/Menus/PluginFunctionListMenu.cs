using MattEland.BatComputer.ConsoleApp.Commands;
using Microsoft.SemanticKernel;

namespace MattEland.BatComputer.ConsoleApp.Menus;

public class PluginFunctionListMenu : MenuBase
{
    public PluginFunctionListMenu(BatComputerApp app, string pluginName) : base(app)
    {
        PluginName = pluginName;
    }

    public override IEnumerable<AppCommand> Commands
    {
        get
        {
            foreach (var func in Kernel.Functions.GetFunctionViews()
                .Where(f => f.PluginName == PluginName && !App.IsFunctionExcluded(f))
                .OrderBy(f => f.Name))
            {
                if (Kernel.Functions.TryGetFunction(func.PluginName, func.Name, out ISKFunction? skFunc))
                {
                    yield return new ExecuteFunctionCommand(App, func, skFunc);
                }
            }

            yield return new ExitCommand(App, title: "Back");
        }
    }

    public string PluginName { get; }
}
