using Microsoft.SemanticKernel;

namespace MattEland.BatComputer.Abstractions.Strategies;

public class NoPlannerStrategy : PlannerStrategy
{
    public override Planner? BuildPlanner(IKernel kernel,
        IEnumerable<string> excludedPlugins,
        IEnumerable<string> excludedFunctions)
    {
        return null;
    }

    public override string ToString() => "No Planner";
}