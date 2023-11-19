using Microsoft.SemanticKernel.Planners;

namespace MattEland.BatComputer.Abstractions.Strategies;

public class StepwisePlannerWrapper : Planner
{
    private readonly StepwisePlanner _planner;

    public StepwisePlannerWrapper(StepwisePlanner planner)
    {
        _planner = planner;
    }

    public override Task<PlanWrapper> CreatePlanAsync(string goal)
        => Task.FromResult(new PlanWrapper(_planner.CreatePlan(goal)));
}