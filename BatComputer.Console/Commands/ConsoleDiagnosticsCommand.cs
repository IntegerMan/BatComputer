using Dumpify;
using Spectre.Console;

namespace MattEland.BatComputer.ConsoleApp.Commands;

public class ConsoleDiagnosticsCommand : AppCommand
{
    public override Task ExecuteAsync()
    {
        AnsiConsole.Profile.Capabilities.Dump(label: $"{AnsiConsole.Profile.Width}x{AnsiConsole.Profile.Height} Console Dimensions");

        return Task.CompletedTask;
    }

    public ConsoleDiagnosticsCommand(BatComputerApp app) : base(app)
    {
    }

    public override string DisplayText => "Display Console Capabilities";
}
