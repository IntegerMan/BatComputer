using Spectre.Console;

namespace MattEland.BatComputer.ConsoleApp;

public sealed class BatComputerSkin : ConsoleSkin {
    public override string AppName => "Bat Computer";
    public override string AppNamePrefix => "the";
    public override Spinner Spinner => Spinner.Known.Default;
}
