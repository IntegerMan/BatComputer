using MattEland.BatComputer.ConsoleApp.Abstractions;
using Spectre.Console;

namespace MattEland.BatComputer.ConsoleApp.Skins;

public sealed class BatComputerSkin : ConsoleSkin {
    public override string AppName => "Bat Computer";
    public override string AppNamePrefix => "the";
    public override Spinner Spinner => Spinner.Known.Default;

    public override string NormalStyle => "Yellow";
    public override Color NormalColor => Color.Yellow;

    public override string SuccessStyle => "gold1";
    public override string AgentStyle => "skyblue3";

    public override string AgentName => "Alfred";

    public override string AccentStyle => "steelblue";
    public override Color AccentColor => Color.SteelBlue;
    public override string WarningStyle => "darkorange";

    public override Color ChartColor1 => Color.LightGoldenrod2_1;
    public override Color ChartColor2 => Color.DarkGoldenrod;
}
