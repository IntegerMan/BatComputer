using MattEland.BatComputer.ConsoleApp.Renderables;

namespace MattEland.BatComputer.ConsoleApp.Commands;

public class ShowPlanTreeCommand : AppCommand
{
    public override Task ExecuteAsync()
    {
        App.LastPlan!.RenderTree(Skin);

        return Task.CompletedTask;
    }

    public override bool CanExecute() => App.LastPlan != null;

    public ShowPlanTreeCommand(BatComputerApp app) : base(app)
    {
    }

    public override string DisplayText => "Show Plan Tree";
}
