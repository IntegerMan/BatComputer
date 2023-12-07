using MattEland.BatComputer.ConsoleApp.Commands;
using Microsoft.SemanticKernel;

namespace MattEland.BatComputer.ConsoleApp.Menus;

public abstract class MenuBase
{
    protected MenuBase(BatComputerApp app)
    {
        App = app;
    }

    public BatComputerApp App { get; }
    public IKernel Kernel => App.Kernel!;

    public abstract IEnumerable<AppCommand> Commands { get; }
}