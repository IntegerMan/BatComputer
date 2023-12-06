using Microsoft.SemanticKernel.Planning;

namespace MattEland.BatComputer.Abstractions.Strategies;

public abstract class Planner
{
    public abstract Task<Plan> CreatePlanAsync(string goal);
}
