using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Planners;

namespace MattEland.BatComputer.Abstractions.Strategies;

public abstract class PlannerStrategy
{
    public abstract Planner? BuildPlanner(IKernel kernel, 
        IEnumerable<string> excludedPlugins, 
        IEnumerable<string> excludedFunctions);

    protected static void ExcludeFunctions(PlannerConfigBase config, 
        IEnumerable<string> excludedPlugins, 
        IEnumerable<string> excludedFunctions)
    {
        foreach (string plugin in excludedPlugins)
        {
            config.ExcludedPlugins.Add(plugin);
        }

        foreach (string function in excludedFunctions)
        {
            config.ExcludedFunctions.Add(function);
        }
    }

}
