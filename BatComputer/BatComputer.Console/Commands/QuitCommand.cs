using MattEland.BatComputer.Kernel;
using Spectre.Console;

namespace MattEland.BatComputer.ConsoleApp.Commands;
public class QuitCommand : AppCommand
{
    public override Task ExecuteAsync(AppKernel kernel)
    {
        // TODO: I could re-implement some of the built-in confirm dialog to expose the choice style so it's not blue by default
        App.ExitRequested = AnsiConsole.Confirm("Are you sure you want to quit?");

        return Task.CompletedTask;
    }

    public override string DisplayText => "Quit";

    public QuitCommand(BatComputerApp app) : base(app)
    {
    }
}
