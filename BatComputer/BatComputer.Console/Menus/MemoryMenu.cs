using MattEland.BatComputer.ConsoleApp.Commands;

namespace MattEland.BatComputer.ConsoleApp.Menus;

public class MemoryMenu : MenuBase
{
    public MemoryMenu(BatComputerApp app) : base(app)
    {
    }

    public override IEnumerable<AppCommand> Commands
    {
        get
        {
            yield return new ShowMemoryCommand(App);

            yield return new ExitCommand(App, title: "Back");
        }
    }
}
