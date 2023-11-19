using Microsoft.SemanticKernel.Planners;
using Microsoft.SemanticKernel.Planners.Handlebars;
using Microsoft.SemanticKernel.Planning;

namespace MattEland.BatComputer.Abstractions.Strategies;

public class HandlebarsPlannerWrapper : Planner
{
    private readonly HandlebarsPlanner _planner;

    public HandlebarsPlannerWrapper(HandlebarsPlanner planner)
    {
        _planner = planner;
    }

    public override async Task<PlanWrapper> CreatePlanAsync(string goal)
        => new PlanWrapper(await _planner.CreatePlanAsync(goal));
}