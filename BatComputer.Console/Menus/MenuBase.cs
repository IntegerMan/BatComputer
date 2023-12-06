using MattEland.BatComputer.ConsoleApp.Commands;

namespace MattEland.BatComputer.ConsoleApp.Menus;

public abstract class MenuBase
{
    protected MenuBase(BatComputerApp app)
    {
        App = app;
    }

    public BatComputerApp App { get; }
    public abstract IEnumerable<AppCommand> Commands { get; }
}