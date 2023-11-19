using Microsoft.SemanticKernel.Planning;

namespace MattEland.BatComputer.Abstractions.Strategies;

public class FunctionCallingStepwisePlannerWrapper : Planner
{
    private FunctionCallingStepwisePlanner planner;

    public FunctionCallingStepwisePlannerWrapper(FunctionCallingStepwisePlanner planner)
    {
        this.planner = planner;
    }
    public override Task<Plan> CreatePlanAsync(string goal)
        => Task.FromResult(_planner.CreatePlan(goal));
}