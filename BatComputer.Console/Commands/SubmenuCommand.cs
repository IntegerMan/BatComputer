using MattEland.BatComputer.ConsoleApp.Menus;
using MattEland.BatComputer.Kernel;

namespace MattEland.BatComputer.ConsoleApp.Commands;

/// <summary>
/// A command that adds a new submenu to the stack
/// </summary>
public class SubmenuCommand : AppCommand
{
    public string Title { get; }
    
    public override Task ExecuteAsync()
    {
        App.Menus.Push(Submenu);

        return Task.CompletedTask;
    }

    public override bool CanExecute() => Submenu.Commands.Any(c => c.CanExecute());

    public override string DisplayText => $"{Title}…";

    public MenuBase Submenu { get; }

    public SubmenuCommand(BatComputerApp app, string title, MenuBase submenu) : base(app)
    {
        Title = title;
        Submenu = submenu;
    }
}
