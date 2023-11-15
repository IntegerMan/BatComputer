using MattEland.BatComputer.Kernel;
using Spectre.Console;

namespace MattEland.BatComputer.ConsoleApp.Commands;

/// <summary>
/// A command to exit the current menu or exit the application
/// </summary>
public class ExitCommand : AppCommand
{
    public string Title { get; }
    public bool Confirm { get; }

    public override Task ExecuteAsync(AppKernel kernel)
    {
        // TODO: I could re-implement some of the built-in confirm dialog to expose the choice style so it's not blue by default
        if (!Confirm || AnsiConsole.Confirm("Are you sure you want to quit?"))
        {
            App.Menus.Pop();
        }

        return Task.CompletedTask;
    }

    public override string DisplayText => Title;

    public ExitCommand(BatComputerApp app, string title, bool confirm = false) : base(app)
    {
        Title = title;
        Confirm = confirm;
    }
}
