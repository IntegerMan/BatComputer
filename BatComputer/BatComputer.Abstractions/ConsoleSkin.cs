using Spectre.Console;

namespace MattEland.BatComputer.Abstractions;

public abstract class ConsoleSkin {
    public abstract string AppName { get; }
    public virtual string AppNamePrefix => string.Empty;
    public virtual Spinner Spinner => Spinner.Known.Default;
    public virtual string NormalStyle => "White";
    public virtual Spectre.Console.Color NormalColor => Spectre.Console.Color.White;
    public virtual string AccentStyle => "Blue";
    public virtual Spectre.Console.Color AccentColor => Spectre.Console.Color.Blue;
    public virtual string ErrorStyle => "Red";
    public virtual string ErrorEmoji => ":stop_sign:";
    public virtual string WarningStyle => "Yellow";
    public virtual string WarningEmoji => ":yellow_circle:";
    public virtual string SuccessStyle => "Green";
    public virtual string SuccessEmoji => ":check_mark_button:";
    public virtual string AgentStyle => "Cyan";
    public virtual string DebugStyle => "grey69";
    public virtual string AgentName => "Bot";

    public string AppNameWithPrefix => string.IsNullOrWhiteSpace(AppNamePrefix) 
            ? AppName 
            : $"{AppNamePrefix} {AppName}";
}
