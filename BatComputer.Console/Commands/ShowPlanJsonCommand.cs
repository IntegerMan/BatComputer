using MattEland.BatComputer.ConsoleApp.Helpers;

namespace MattEland.BatComputer.ConsoleApp.Commands;

public class ShowPlanJsonCommand : AppCommand
{
    public override Task ExecuteAsync()
    {
        App.LastPlan!.RenderJson();

        return Task.CompletedTask;
    }

    public override bool CanExecute() => App.LastPlan != null;

    public ShowPlanJsonCommand(BatComputerApp app) : base(app)
    {
    }

    public override string DisplayText => "Show Plan JSON";
}
