using MattEland.BatComputer.Abstractions.Strategies;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Planning;
using Microsoft.SemanticKernel.Planners;

namespace MattEland.BatComputer.ConsoleApp;

public class SamplePlannerProvider : IPlannerProvider
{
    public Task<Plan> CreatePlanAsync(string goal, IKernel kernel, IEnumerable<FunctionView> functions)
    {
        StepwisePlannerConfig config = new StepwisePlannerConfig()
        {
            GetAvailableFunctionsAsync = (config, name, token) => 
            {
                return Task.FromResult(functions);
            }
        };

        var planner = new StepwisePlanner(kernel, config);
        return Task.FromResult(planner.CreatePlan(goal));
    }
}
