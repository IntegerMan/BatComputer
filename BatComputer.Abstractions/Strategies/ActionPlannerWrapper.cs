using Microsoft.SemanticKernel.Planners;
using Microsoft.SemanticKernel.Planning;

namespace MattEland.BatComputer.Abstractions.Strategies;

public class ActionPlannerWrapper : Planner
{
    private readonly ActionPlanner _planner;

    public ActionPlannerWrapper(ActionPlanner planner)
    {
        _planner = planner;
    }

    public override async Task<Plan> CreatePlanAsync(string goal)
    {
        return await _planner.CreatePlanAsync(goal);
    }
}