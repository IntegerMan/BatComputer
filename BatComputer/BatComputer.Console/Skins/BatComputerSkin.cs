using Spectre.Console;

namespace MattEland.BatComputer.ConsoleApp.Skins;

public sealed class BatComputerSkin : ConsoleSkin {
    public override string AppName => "Bat Computer";
    public override string AppNamePrefix => "the";
    public override Spinner Spinner => Spinner.Known.Default;

    public override string NormalStyle => "Yellow";
    public override Color NormalColor => Color.Yellow;

    public override string SuccessStyle => "gold1";
    public override string AgentStyle => "steelblue";

    public override string AgentName => "Alfred";

    public override string AccentStyle => "darkorange";
    public override Color AccentColor => Color.DarkOrange;
}
