using Spectre.Console;

namespace MattEland.BatComputer.ConsoleApp;

public sealed class BatComputerSkin : ConsoleSkin {
    public override string AppName => "Bat Computer";
    public override string AppNamePrefix => "the";
    public override Spinner Spinner => Spinner.Known.Default;

    public override string NormalStyle => "Yellow";
    public override Color NormalColor { get; } = Color.Yellow;

    public override string SuccessStyle => "gold1";
    public override string AgentStyle => "steelblue";
}
