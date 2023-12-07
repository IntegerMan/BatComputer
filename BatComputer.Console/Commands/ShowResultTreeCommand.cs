using MattEland.BatComputer.ConsoleApp.Renderables;

namespace MattEland.BatComputer.ConsoleApp.Commands;

public class ShowResultTreeCommand : AppCommand
{
    public override Task ExecuteAsync()
    {
        App.LastResult!.RenderTree(Skin);

        return Task.CompletedTask;
    }

    public override bool CanExecute() => App.LastResult != null;

    public ShowResultTreeCommand(BatComputerApp app) : base(app)
    {
    }

    public override string DisplayText => "Explain your last response";
}
