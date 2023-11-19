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
            yield return new VoiceCommand(App);
            yield return new ShowResultTreeCommand(App);
            yield return new RetryCommand(App);
            yield return new ChangePlannerCommand(App);

            yield return new SubmenuCommand(App, "Diagnostics", new DiagnosticsMenu(App));
            
            yield return new ExitCommand(App, title: "Quit", confirm: true);
        }
    }
}
