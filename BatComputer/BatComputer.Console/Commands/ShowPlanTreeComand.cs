using MattEland.BatComputer.ConsoleApp.Renderables;
using MattEland.BatComputer.Kernel;

namespace MattEland.BatComputer.ConsoleApp.Commands;

public class ShowPlanTreeCommand : AppCommand
{
    public override Task ExecuteAsync(AppKernel kernel)
    {
        kernel.LastPlan!.Plan!.RenderTree(Skin);

        return Task.CompletedTask;
    }

    public override bool CanExecute(AppKernel kernel) => kernel.LastPlan?.Plan != null;

    public ShowPlanTreeCommand(BatComputerApp app) : base(app)
    {
    }

    public override string DisplayText => "Show Plan Tree";
}
