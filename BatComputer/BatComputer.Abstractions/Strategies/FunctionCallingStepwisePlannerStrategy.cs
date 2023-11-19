using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Planners;

namespace MattEland.BatComputer.Abstractions.Strategies;

public class FunctionCallingStepwisePlannerStrategy : PlannerStrategy
{
    public override Planner? BuildPlanner(IKernel kernel, IEnumerable<string> excludedPlugins, IEnumerable<string> excludedFunctions)
    {
        FunctionCallingStepwisePlannerConfig config = new();

        ExcludeFunctions(config, excludedPlugins, excludedFunctions);

        FunctionCallingStepwisePlanner planner = new(kernel, config);

        return new FunctionCallingStepwisePlannerWrapper(planner);
    }

    public override string ToString() => "Function Calling Stepwise Planner";
}
