using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Planners;

namespace MattEland.BatComputer.Abstractions.Strategies;

public class StepwisePlannerStrategy : PlannerStrategy
{
    public override Planner? BuildPlanner(IKernel kernel, 
        IEnumerable<string>? excludedPlugins, 
        IEnumerable<string>? excludedFunctions)
    {
        StepwisePlannerConfig config = new();

        ExcludeFunctions(config, excludedPlugins, excludedFunctions);

        StepwisePlanner planner = new(kernel, config);

        return new StepwisePlannerWrapper(planner);
    }

    public override string ToString() => "Stepwise Planner";
}
