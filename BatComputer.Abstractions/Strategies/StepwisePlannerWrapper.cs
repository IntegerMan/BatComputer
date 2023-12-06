using Microsoft.SemanticKernel.Planners;
using Microsoft.SemanticKernel.Planning;

namespace MattEland.BatComputer.Abstractions.Strategies;

public class StepwisePlannerWrapper : Planner
{
    private readonly StepwisePlanner _planner;

    public StepwisePlannerWrapper(StepwisePlanner planner)
    {
        _planner = planner;
    }

    public override Task<Plan> CreatePlanAsync(string goal) 
        => Task.FromResult(_planner.CreatePlan(goal));
}