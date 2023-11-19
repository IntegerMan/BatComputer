using MattEland.BatComputer.ConsoleApp.Abstractions;
using MattEland.BatComputer.Kernel;

namespace MattEland.BatComputer.ConsoleApp.Commands;

public abstract class AppCommand
{
    public BatComputerApp App { get; }

    public ConsoleSkin Skin => App.Skin;

    protected AppCommand(BatComputerApp app)
    {
        App = app;
    }

    public virtual string DisplayText => GetType().Name;

    public virtual bool CanExecute(AppKernel kernel) => true;

    public abstract Task ExecuteAsync(AppKernel kernel);

    public override string ToString() => DisplayText;
}
