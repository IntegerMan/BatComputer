using MattEland.BatComputer.ConsoleApp.Renderables;
using MattEland.BatComputer.Kernel;

namespace MattEland.BatComputer.ConsoleApp.Commands;

public class ShowResultTreeCommand : AppCommand
{
    public override Task ExecuteAsync(AppKernel kernel)
    {
        kernel.LastResult!.RenderTree(Skin);

        return Task.CompletedTask;
    }

    public override bool CanExecute(AppKernel kernel) => kernel.LastResult != null;

    public ShowResultTreeCommand(BatComputerApp app) : base(app)
    {
    }

    public override string DisplayText => "Explain your last response";
}
