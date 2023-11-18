using MattEland.BatComputer.Abstractions.Strategies;
using MattEland.BatComputer.Abstractions.Widgets;
using MattEland.BatComputer.ConsoleApp.Helpers;
using MattEland.BatComputer.ConsoleApp.Renderables;
using MattEland.BatComputer.Kernel;
using Spectre.Console;

namespace MattEland.BatComputer.ConsoleApp.Commands;

public class ShowExplanationCommand : AppCommand
{
    public override Task ExecuteAsync(AppKernel kernel)
    {
        int step = 1;
        foreach (StepwiseSummary summary in kernel.LastResult?.Summary.Where(t => !string.IsNullOrWhiteSpace(t.Thought)) ?? Enumerable.Empty<StepwiseSummary>())
        {
            Panel box = new Panel($"[{Skin.DebugStyle}] {Markup.Escape(summary.Thought!)} [/]")
            .Border(BoxBorder.Rounded)
            .BorderStyle(Skin.AccentStyle)
            .Header($"[{Skin.NormalStyle}] Step {step++}: {Markup.Escape(summary.Action ?? "")} [/]", Justify.Left);

            AnsiConsole.Write(box);
            AnsiConsole.WriteLine();
        }

        return Task.CompletedTask;
    }

    public override bool CanExecute(AppKernel kernel) 
        => kernel.LastResult?.Summary.Select(s => s.Thought).Any(t => !string.IsNullOrEmpty(t)) ?? false;

    public ShowExplanationCommand(BatComputerApp app) : base(app)
    {
    }

    public override string DisplayText => "Explain your last response";
}
