using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Planners;

namespace MattEland.BatComputer.Abstractions.Strategies;

public abstract class PlannerStrategy
{
    public abstract Planner? BuildPlanner(IKernel kernel, 
        IEnumerable<string>? excludedPlugins = null, 
        IEnumerable<string>? excludedFunctions = null);

    protected static void ExcludeFunctions(PlannerConfigBase config, 
        IEnumerable<string>? excludedPlugins, 
        IEnumerable<string>? excludedFunctions)
    {
        foreach (string plugin in excludedPlugins ?? Enumerable.Empty<string>())
        {
            config.ExcludedPlugins.Add(plugin);
        }

        foreach (string function in excludedFunctions ?? Enumerable.Empty<string>())
        {
            config.ExcludedFunctions.Add(function);
        }
    }

}
