using Spectre.Console;

namespace BatComputer.Abstractions;

public abstract class ConsoleSkin {
    public abstract string AppName { get; }
    public virtual string AppNamePrefix { get; } = string.Empty;
    public virtual Spinner Spinner { get; } = Spinner.Known.Default;
    public virtual string NormalStyle { get; } = "White";
    public virtual Spectre.Console.Color NormalColor { get; } = Spectre.Console.Color.White;
    public virtual string AccentStyle { get; } = "Blue";
    public virtual Spectre.Console.Color AccentColor { get; } = Spectre.Console.Color.Blue;
    public virtual string ErrorStyle { get; } = "Red";
    public virtual string ErrorEmoji { get; } = ":stop_sign:";
    public virtual string SuccessStyle { get; } = "Green";
    public virtual string SuccessEmoji { get; } = ":check_mark_button:";
    public virtual string AgentStyle { get; } = "Cyan";
    public virtual string DebugStyle { get; } = "grey69";
    public virtual string AgentName { get; } = "Bot";
}
