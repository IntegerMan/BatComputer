using MattEland.BatComputer.ConsoleApp.Helpers;
using MattEland.BatComputer.Kernel;

namespace MattEland.BatComputer.ConsoleApp.Commands;

public class ShowPlanJsonCommand : AppCommand
{
    public override Task ExecuteAsync(AppKernel kernel)
    {
        kernel.LastPlan!.RenderJson();

        return Task.CompletedTask;
    }

    public override bool CanExecute(AppKernel kernel) => kernel.LastPlan != null;

    public ShowPlanJsonCommand(BatComputerApp app) : base(app)
    {
    }

    public override string DisplayText => "Show Plan JSON";
}
