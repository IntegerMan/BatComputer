using Microsoft.SemanticKernel.Planning;

namespace MattEland.BatComputer.Abstractions.Strategies;

public abstract class Planner
{
    public abstract Task<PlanWrapper> CreatePlanAsync(string goal);
}
