using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Planners;
using Microsoft.SemanticKernel.Planners.Handlebars;

namespace MattEland.BatComputer.Abstractions.Strategies;

public class HandlebarsPlannerStrategy : PlannerStrategy
{
    public override Planner? BuildPlanner(IKernel kernel, IEnumerable<string> excludedPlugins, IEnumerable<string> excludedFunctions)
    {
        HandlebarsPlannerConfig config = new();

        ExcludeFunctions(config, excludedPlugins, excludedFunctions);

        HandlebarsPlanner planner = new(kernel, config);

        return new HandlebarsPlannerWrapper(planner);
    }

    public override string ToString() => "Handlebars Planner";
}
