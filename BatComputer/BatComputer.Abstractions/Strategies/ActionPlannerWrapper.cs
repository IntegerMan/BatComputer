using Microsoft.SemanticKernel.Planners;

namespace MattEland.BatComputer.Abstractions.Strategies;

public class ActionPlannerWrapper : Planner
{
    private readonly ActionPlanner _planner;

    public ActionPlannerWrapper(ActionPlanner planner)
    {
        _planner = planner;
    }

    public override async Task<PlanWrapper> CreatePlanAsync(string goal) 
        => new PlanWrapper(await _planner.CreatePlanAsync(goal));
}