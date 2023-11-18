using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Planners;

namespace MattEland.BatComputer.Abstractions.Strategies;

public class ActionPlannerStrategy : PlannerStrategy
{
    public override Planner? BuildPlanner(IKernel kernel, IEnumerable<string> excludedPlugins, IEnumerable<string> excludedFunctions)
    {
        ActionPlannerConfig config = new();

        ExcludeFunctions(config, excludedPlugins, excludedFunctions);

        ActionPlanner planner = new(kernel, config);

        return new ActionPlannerWrapper(planner);
    }

    public override string ToString() => "Action Planner";
}
