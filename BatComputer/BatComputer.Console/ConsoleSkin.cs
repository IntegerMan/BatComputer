using Spectre.Console;

namespace MattEland.BatComputer.ConsoleApp;

public abstract class ConsoleSkin {
    public abstract string AppName { get; }
    public virtual string AppNamePrefix { get; } = string.Empty;
    public virtual Spinner Spinner { get; } = Spinner.Known.Default;
}
