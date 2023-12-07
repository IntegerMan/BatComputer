using MattEland.BatComputer.Abstractions.Strategies;
using MattEland.BatComputer.ConsoleApp.Abstractions;
using Spectre.Console;

namespace MattEland.BatComputer.ConsoleApp.Commands;

/// <summary>
/// A command to reselect the planner. This is useful when demoing different planners or evaluating commands on different planners.
/// </summary>
public class ChangePlannerCommand : AppCommand
{
    public override Task ExecuteAsync()
    {
        PlannerStrategy strategy = SelectPlanner(Skin);
        App.Planner = strategy.BuildPlanner(Kernel);

        return Task.CompletedTask;
    }

    public static PlannerStrategy SelectPlanner(ConsoleSkin skin)
    {
        SelectionPrompt<PlannerStrategy> choices = new SelectionPrompt<PlannerStrategy>()
        .Title($"[{skin.NormalStyle}]Select a planner[/]")
        .HighlightStyle(skin.AccentStyle)
        .AddChoices([
            new StepwisePlannerStrategy(),
            new SequentialPlannerStrategy(),
            new ActionPlannerStrategy(),
            new NoPlannerStrategy()
        ]);

        return AnsiConsole.Prompt(choices);
    }

    public override string DisplayText => "Change Planner…";

    public ChangePlannerCommand(BatComputerApp app) : base(app)
    {
    }
}
