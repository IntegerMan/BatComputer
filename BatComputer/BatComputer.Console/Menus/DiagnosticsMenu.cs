using MattEland.BatComputer.ConsoleApp.Commands;

namespace MattEland.BatComputer.ConsoleApp.Menus;

public class DiagnosticsMenu : MenuBase
{
    public DiagnosticsMenu(BatComputerApp app) : base(app)
    {
    }

    public override IEnumerable<AppCommand> Commands
    {
        get
        {
            yield return new ConsoleDiagnosticsCommand(App);
            yield return new ShowPlanTreeCommand(App);
            yield return new ShowPlanJsonCommand(App);
            yield return new SubmenuCommand(App, "Functions", new FunctionsMenu(App));
            yield return new SubmenuCommand(App, "Widgets", new WidgetsMenu(App));
            yield return new ExitCommand(App, title: "Back");
        }
    }
}
