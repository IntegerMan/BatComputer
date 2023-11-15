using Azure;
using MattEland.BatComputer.ConsoleApp.Helpers;
using MattEland.BatComputer.ConsoleApp.Renderables;
using MattEland.BatComputer.Kernel;

namespace MattEland.BatComputer.ConsoleApp.Commands;

public class ListPluginsCommand : AppCommand
{
    public override Task ExecuteAsync(AppKernel kernel)
    {
        kernel.RenderKernelPluginsTable(Skin);

        return Task.CompletedTask;
    }

    public ListPluginsCommand(BatComputerApp app) : base(app)
    {
    }

    public override string DisplayText => "List Plugins";
}
