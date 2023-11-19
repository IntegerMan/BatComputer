using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.Planners.Handlebars;
using Microsoft.SemanticKernel.Planning;

namespace MattEland.BatComputer.Abstractions.Strategies;

public class PlanWrapper
{
    public PlanWrapper(HandlebarsPlan plan)
    {
        HandlebarsPlan = plan;
    }

    public PlanWrapper(Plan plan)
    {
        Plan = plan;
    }

    public async Task<FunctionResult> InvokeAsync(IKernel kernel)
    {
        if (Plan != null)
        {
            return await Plan.InvokeAsync(kernel);
        }
        else if (HandlebarsPlan != null)
        {
            SKContext context = kernel.CreateNewContext();
            Dictionary<string, object?> variables = new();
            return HandlebarsPlan.Invoke(context, variables);
        }
        else
        {
            throw new InvalidOperationException("No plan was provided");
        }
    }

    public Plan? Plan { get; }
    public HandlebarsPlan? HandlebarsPlan { get; }
}