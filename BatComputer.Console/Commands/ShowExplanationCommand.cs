using MattEland.BatComputer.Abstractions.Strategies;
using MattEland.BatComputer.Kernel;
using Spectre.Console;

namespace MattEland.BatComputer.ConsoleApp.Commands;

public class ShowExplanationCommand : AppCommand
{
    public override Task ExecuteAsync()
    {
        int step = 1;
        foreach (StepSummary summary in App.LastResult?.Summary.Where(t => !string.IsNullOrWhiteSpace(t.Thought)) ?? Enumerable.Empty<StepSummary>())
        {
            if (string.IsNullOrEmpty(summary.Thought))
                continue;

            Panel box = new Panel($"[{Skin.DebugStyle}] {Markup.Escape(summary.Thought!)} [/]")
            .Border(BoxBorder.Rounded)
            .BorderStyle(Skin.AccentStyle)
            .Header($"[{Skin.NormalStyle}] Step {step++}: {Markup.Escape(summary.Action ?? "")} [/]", Justify.Left);

            AnsiConsole.Write(box);
            AnsiConsole.WriteLine();
        }

        return Task.CompletedTask;
    }

    public override bool CanExecute() 
        => App.LastResult?.Summary.Select(s => s.Thought).Any(t => !string.IsNullOrEmpty(t)) ?? false;

    public ShowExplanationCommand(BatComputerApp app) : base(app)
    {
    }

    public override string DisplayText => "Explain your last response";
}
