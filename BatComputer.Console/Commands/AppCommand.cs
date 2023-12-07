using MattEland.BatComputer.ConsoleApp.Abstractions;
using Microsoft.SemanticKernel;

namespace MattEland.BatComputer.ConsoleApp.Commands;

public abstract class AppCommand
{
    public BatComputerApp App { get; }

    public IKernel Kernel => App.Kernel!;

    public ConsoleSkin Skin => App.Skin;

    protected AppCommand(BatComputerApp app)
    {
        App = app;
    }

    public virtual string DisplayText => GetType().Name;

    public virtual bool CanExecute() => true;

    public abstract Task ExecuteAsync();

    public override string ToString() => DisplayText;
}
