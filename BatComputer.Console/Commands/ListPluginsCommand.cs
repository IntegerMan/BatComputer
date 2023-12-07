using MattEland.BatComputer.ConsoleApp.Renderables;

namespace MattEland.BatComputer.ConsoleApp.Commands;

public class ListPluginsCommand : AppCommand
{
    public override Task ExecuteAsync()
    {
        KernelPluginsRenderer.RenderKernelPluginsTable(Kernel, Skin, App);

        return Task.CompletedTask;
    }

    public ListPluginsCommand(BatComputerApp app) : base(app)
    {
    }

    public override string DisplayText => "List Plugins";
}
