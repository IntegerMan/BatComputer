using MattEland.BatComputer.Abstractions;
using MattEland.BatComputer.Abstractions.Widgets;
using MattEland.BatComputer.ConsoleApp.Commands;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI.AzureSdk;

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

            foreach (var func in App.Kernel!.Kernel.Functions.GetFunctionViews())
            {
                if (App.Kernel!.Kernel.Functions.TryGetFunction(func.PluginName, func.Name, out ISKFunction? skFunc))
                {
                    yield return new ExecuteFunctionCommand(App, func, skFunc);
                }
            }

            yield return new ExitCommand(App, title: "Back");
        }
    }
}
